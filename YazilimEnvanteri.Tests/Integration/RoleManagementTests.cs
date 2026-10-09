using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Models.Identity;

namespace YazilimEnvanteri.Tests.Integration;

public class RoleManagementTests(PostgresFixture db) : IntegrationTestBase(db)
{
    [Theory]
    [InlineData(AppRoles.SuperAdmin, HttpStatusCode.OK)]
    [InlineData(AppRoles.Admin, HttpStatusCode.Forbidden)]
    [InlineData(AppRoles.Developer, HttpStatusCode.Forbidden)]
    [InlineData(AppRoles.Observer, HttpStatusCode.Forbidden)]
    public async Task Only_superadmin_can_open_role_management(string role, HttpStatusCode expected)
    {
        var client = await ClientAsAsync(role);

        Assert.Equal(expected, (await client.GetAsync("/RoleManagement")).StatusCode);
    }

    [Theory]
    [InlineData(AppRoles.Admin)]
    [InlineData(AppRoles.Developer)]
    [InlineData(AppRoles.Observer)]
    public async Task Non_superadmin_cannot_change_roles_even_by_posting_directly(string role)
    {
        var target = await CreateUserAsync(AppRoles.Observer);
        var client = await ClientAsAsync(role);

        var response = await PostChangeRoleAsync(client, target.Id, AppRoles.SuperAdmin);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal([AppRoles.Observer], await RolesOfAsync(target.Id));
    }

    [Fact]
    public async Task Superadmin_can_replace_a_users_roles()
    {
        var target = await CreateUserAsync(AppRoles.Observer);
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        var response = await PostChangeRoleAsync(client, target.Id, AppRoles.Admin, AppRoles.Developer);

        response.EnsureSuccessStatusCode(); // redirected back to the list
        Assert.Equal([AppRoles.Admin, AppRoles.Developer], (await RolesOfAsync(target.Id)).Order());
    }

    [Theory]
    [InlineData("Hacker")]
    [InlineData("superadmin")] // role names are matched exactly
    public async Task Unknown_role_names_are_rejected(string bogus)
    {
        var target = await CreateUserAsync(AppRoles.Observer);
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        await PostChangeRoleAsync(client, target.Id, AppRoles.Developer, bogus);

        Assert.Equal([AppRoles.Observer], await RolesOfAsync(target.Id));
        using var scope = Db.App.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().RoleExistsAsync("Hacker"));
    }

    [Fact]
    public async Task Empty_role_selection_is_rejected()
    {
        var target = await CreateUserAsync(AppRoles.Developer);
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        await PostChangeRoleAsync(client, target.Id);

        Assert.Equal([AppRoles.Developer], await RolesOfAsync(target.Id));
    }

    [Fact]
    public async Task Unknown_user_returns_404()
    {
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        Assert.Equal(HttpStatusCode.NotFound, (await PostChangeRoleAsync(client, "no-such-user", AppRoles.Admin)).StatusCode);
    }

    [Fact]
    public async Task Last_superadmin_cannot_lose_the_role()
    {
        var last = await CreateUserAsync(AppRoles.SuperAdmin);
        await DemoteAllSuperAdminsExceptAsync(last.Id);
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        await PostChangeRoleAsync(client, last.Id, AppRoles.Admin);
        Assert.Equal([AppRoles.SuperAdmin], await RolesOfAsync(last.Id));

        // Once someone else is SuperAdmin, the same change goes through.
        await CreateUserAsync(AppRoles.SuperAdmin);
        await PostChangeRoleAsync(client, last.Id, AppRoles.Admin);
        Assert.Equal([AppRoles.Admin], await RolesOfAsync(last.Id));
    }

    [Fact]
    public async Task Page_lists_users_without_sensitive_identity_fields()
    {
        var target = await CreateUserAsync(AppRoles.Developer);
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        var html = await client.GetStringAsync("/RoleManagement");

        Assert.Contains(target.Email!, html);
        Assert.DoesNotContain(target.PasswordHash!, html);
        Assert.DoesNotContain(target.SecurityStamp!, html);
    }

    [Fact]
    public async Task Superadmin_gets_a_one_time_temporary_password_that_works()
    {
        string userId;
        using (var scope = Db.App.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var email = $"personel{Guid.NewGuid():N}@example.com";
            var user = new ApplicationUser { UserName = email, Email = email };
            Assert.True((await users.CreateAsync(user)).Succeeded); // as the Personeller sync does: no password
            userId = user.Id;
        }
        var client = await ClientAsAsync(AppRoles.SuperAdmin);

        var before = await client.GetStringAsync("/RoleManagement");
        Assert.Contains("Geçici şifre oluştur", WebUtility.HtmlDecode(before));

        var response = await client.PostAsync("/RoleManagement/ResetPassword",
            new FormUrlEncodedContent([KeyValuePair.Create("userId", userId)]));
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        var password = Regex.Match(html, "data-temp-password>([^<]+)<").Groups[1].Value;

        Assert.Equal(16, password.Length);
        using (var scope = Db.App.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByIdAsync(userId))!;
            Assert.True(await users.CheckPasswordAsync(user, password));
            Assert.DoesNotContain(password, user.PasswordHash!); // stored hashed only
        }

        // Shown once: reloading the page doesn't show it again.
        Assert.DoesNotContain(password, WebUtility.HtmlDecode(await client.GetStringAsync("/RoleManagement")));
    }

    [Theory]
    [InlineData(AppRoles.Admin)]
    [InlineData(AppRoles.Developer)]
    [InlineData(AppRoles.Observer)]
    public async Task Non_superadmin_cannot_reset_passwords(string role)
    {
        var target = await CreateUserAsync(AppRoles.Observer);
        var client = await ClientAsAsync(role);

        var response = await client.PostAsync("/RoleManagement/ResetPassword",
            new FormUrlEncodedContent([KeyValuePair.Create("userId", target.Id)]));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static Task<HttpResponseMessage> PostChangeRoleAsync(HttpClient client, string userId, params string[] roles) =>
        client.PostAsync("/RoleManagement/ChangeRole", new FormUrlEncodedContent(
            roles.Select(r => KeyValuePair.Create("roles", r)).Prepend(KeyValuePair.Create("userId", userId))));

    private async Task<ApplicationUser> CreateUserAsync(string role)
    {
        using var scope = Db.App.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"user{Guid.NewGuid():N}@example.com";
        var user = new ApplicationUser { UserName = email, Email = email, AdSoyad = "Test Kullanıcı" };
        Assert.True((await users.CreateAsync(user, "Test!12345")).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        return user;
    }

    private async Task<List<string>> RolesOfAsync(string userId)
    {
        using var scope = Db.App.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return [.. await users.GetRolesAsync((await users.FindByIdAsync(userId))!)];
    }

    // The test database is shared, so other tests may have left SuperAdmins behind.
    private async Task DemoteAllSuperAdminsExceptAsync(string keepUserId)
    {
        using var scope = Db.App.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var user in await users.GetUsersInRoleAsync(AppRoles.SuperAdmin))
            if (user.Id != keepUserId)
                await users.RemoveFromRoleAsync(user, AppRoles.SuperAdmin);
    }
}
