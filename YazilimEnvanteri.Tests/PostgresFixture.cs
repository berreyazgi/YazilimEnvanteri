using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Testcontainers.PostgreSql;
using YazilimEnvanteri.Data;

namespace YazilimEnvanteri.Tests;

// One throwaway postgres (same image as compose.staging.yml) per test run, migrated with the real
// EF migrations, plus the app booted against it. Shared by every test class in PostgresCollection.
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public string ConnectionString => _container.GetConnectionString();

    public AppFactory App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(ConnectionString).Options;
        await using (var db = new ApplicationDbContext(options))
        {
            await db.Database.MigrateAsync();
        }

        App = new AppFactory(ConnectionString);
    }

    // ponytail: FK triggers off for this session (Birim <-> YazilimUzmani <-> Proje FKs are circular,
    // so seeding a valid graph is a chore). Fine for testing SQL; FK behaviour itself is not covered.
    public NpgsqlDataSource CreateDataSourceWithoutForeignKeys() =>
        NpgsqlDataSource.Create(new NpgsqlConnectionStringBuilder(ConnectionString)
        {
            Options = "-c session_replication_role=replica"
        }.ConnectionString);

    public async Task DisposeAsync()
    {
        await App.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}

public sealed class AppFactory(string connectionString) : WebApplicationFactory<Program>
{
    // UseSetting (not ConfigureAppConfiguration): Program.cs reads the connection string before Build().
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);

        // Requests carrying X-Test-Roles are signed in as a user with those roles; everything else
        // goes through the real Identity cookie. Challenge/forbid stay Identity's, so 401/403
        // handling is the production one.
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, null)
                .AddPolicyScheme("TestOrIdentity", null, o => o.ForwardDefaultSelector = ctx =>
                    ctx.Request.Headers.ContainsKey(TestAuthHandler.RolesHeader) ? TestAuthHandler.SchemeName : IdentityConstants.ApplicationScheme);
            services.Configure<AuthenticationOptions>(o => o.DefaultAuthenticateScheme = "TestOrIdentity");
        });
    }
}

public sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string RolesHeader = "X-Test-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RolesHeader, out var header))
            return Task.FromResult(AuthenticateResult.NoResult());

        var roles = header.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "test-" + header), new(ClaimTypes.Name, "test@example.com") };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}

[Collection(PostgresCollection.Name)]
public abstract class IntegrationTestBase(PostgresFixture db)
{
    protected PostgresFixture Db { get; } = db;

    // Anonymous client (no session).
    protected HttpClient Client { get; } = db.App.CreateClient();

    // Signed in with the given roles and carrying a valid antiforgery token, so a rejected POST
    // proves the role check - not a missing token - stopped it.
    protected async Task<HttpClient> ClientAsAsync(params string[] roles)
    {
        var client = Db.App.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(",", roles));
        client.DefaultRequestHeaders.Add("RequestVerificationToken", await GetAntiforgeryTokenAsync(client, "/"));
        return client;
    }

    // The request token from _Layout's <meta> or a form's hidden field; the matching cookie stays
    // in the client's cookie container.
    protected static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string url)
    {
        var html = await client.GetStringAsync(url);
        var match = Regex.Match(html, "(?:name=\"request-verification-token\" content|name=\"__RequestVerificationToken\" type=\"hidden\" value)=\"([^\"]+)\"");
        Assert.True(match.Success, $"No antiforgery token on {url}");
        return match.Groups[1].Value;
    }
}
