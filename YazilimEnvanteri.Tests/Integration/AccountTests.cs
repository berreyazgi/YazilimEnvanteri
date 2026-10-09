using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Models.Identity;

namespace YazilimEnvanteri.Tests.Integration;

// Real Identity cookie flow (no X-Test-Roles header).
public class AccountTests(PostgresFixture db) : IntegrationTestBase(db)
{
    private HttpClient BrowserClient()
    {
        var client = Db.App.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        return client;
    }

    [Fact]
    public async Task Anonymous_page_visit_redirects_to_login()
    {
        var response = await BrowserClient().GetAsync("/Proje");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location!.PathAndQuery);
    }

    [Fact]
    public async Task Forbidden_page_visit_redirects_to_access_denied()
    {
        var client = BrowserClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, AppRoles.Admin);

        var response = await client.GetAsync("/RoleManagement");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", response.Headers.Location!.PathAndQuery);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(response.Headers.Location)).StatusCode);
    }

    [Fact]
    public async Task Registered_user_is_observer_and_cannot_create_projects()
    {
        var client = BrowserClient();
        var email = $"kayit{Guid.NewGuid():N}@example.com";

        var register = await PostFormAsync(client, "/Account/Register", new()
        {
            ["AdSoyad"] = "Yeni Kullanıcı", ["Email"] = email, ["Password"] = "Test!12345", ["ConfirmPassword"] = "Test!12345",
            ["Role"] = AppRoles.SuperAdmin // mass-assignment attempt: RegisterViewModel has no such field
        });
        Assert.Equal(HttpStatusCode.Redirect, register.StatusCode);

        using (var scope = Db.App.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.Equal([AppRoles.Observer], await users.GetRolesAsync((await users.FindByEmailAsync(email))!));
        }

        // Signed in by the registration: can view, cannot create.
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Proje")).StatusCode);
        var token = await GetAntiforgeryTokenAsync(client, "/Proje");
        var create = new HttpRequestMessage(HttpMethod.Post, "/Proje/Create") { Content = JsonContent.Create(new { projeKodu = "123" }) };
        create.Headers.Add("RequestVerificationToken", token);
        client.DefaultRequestHeaders.Accept.Clear(); // a fetch() call, not a page navigation
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(create)).StatusCode);
    }

    [Fact]
    public async Task Login_and_logout()
    {
        var email = $"giris{Guid.NewGuid():N}@example.com";
        using (var scope = Db.App.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { UserName = email, Email = email, AdSoyad = "Giriş Test" };
            Assert.True((await users.CreateAsync(user, "Test!12345")).Succeeded);
            await users.AddToRoleAsync(user, AppRoles.Developer);
        }
        var client = BrowserClient();

        var wrong = await PostFormAsync(client, "/Account/Login", new() { ["Email"] = email, ["Password"] = "yanlis" });
        Assert.Equal(HttpStatusCode.OK, wrong.StatusCode);
        Assert.Contains("E-posta veya şifre hatalı.", WebUtility.HtmlDecode(await wrong.Content.ReadAsStringAsync()));

        // Off-site returnUrl is ignored (no open redirect).
        var login = await PostFormAsync(client, "/Account/Login?returnUrl=https%3A%2F%2Fevil.example", new() { ["Email"] = email, ["Password"] = "Test!12345" });
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/", login.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Proje")).StatusCode);

        var logout = await PostFormAsync(client, "/Account/Logout", new(), tokenPage: "/Proje");
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Proje")).StatusCode);
    }

    [Fact]
    public async Task User_can_replace_their_password_and_passwordless_accounts_cannot_sign_in()
    {
        var email = $"sifre{Guid.NewGuid():N}@example.com";
        var passwordless = $"sifresiz{Guid.NewGuid():N}@example.com";
        using (var scope = Db.App.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { UserName = email, Email = email };
            Assert.True((await users.CreateAsync(user, "Gecici!123")).Succeeded);
            await users.AddToRoleAsync(user, AppRoles.Observer);
            Assert.True((await users.CreateAsync(new ApplicationUser { UserName = passwordless, Email = passwordless })).Succeeded);
        }
        var client = BrowserClient();

        var noPassword = await PostFormAsync(client, "/Account/Login", new() { ["Email"] = passwordless, ["Password"] = "Gecici!123" });
        Assert.Equal(HttpStatusCode.OK, noPassword.StatusCode); // back on the form with an error

        await PostFormAsync(client, "/Account/Login", new() { ["Email"] = email, ["Password"] = "Gecici!123" });
        var change = await PostFormAsync(client, "/Account/ChangePassword", new()
        {
            ["CurrentPassword"] = "Gecici!123", ["NewPassword"] = "Kalici!456", ["ConfirmPassword"] = "Kalici!456"
        });
        Assert.Equal(HttpStatusCode.Redirect, change.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Proje")).StatusCode); // this session survives

        var fresh = BrowserClient();
        var oldLogin = await PostFormAsync(fresh, "/Account/Login", new() { ["Email"] = email, ["Password"] = "Gecici!123" });
        Assert.Equal(HttpStatusCode.OK, oldLogin.StatusCode);
        var newLogin = await PostFormAsync(fresh, "/Account/Login", new() { ["Email"] = email, ["Password"] = "Kalici!456" });
        Assert.Equal(HttpStatusCode.Redirect, newLogin.StatusCode);
    }

    [Fact]
    public async Task Post_without_antiforgery_token_is_rejected()
    {
        var response = await Client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "a@example.com", ["Password"] = "x"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostFormAsync(HttpClient client, string url, Dictionary<string, string> fields, string? tokenPage = null)
    {
        fields["__RequestVerificationToken"] = await GetAntiforgeryTokenAsync(client, tokenPage ?? url);
        return await client.PostAsync(url, new FormUrlEncodedContent(fields));
    }
}
