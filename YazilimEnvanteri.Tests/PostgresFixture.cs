using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
}

[Collection(PostgresCollection.Name)]
public abstract class IntegrationTestBase(PostgresFixture db)
{
    protected PostgresFixture Db { get; } = db;
    protected HttpClient Client { get; } = db.App.CreateClient();
}
