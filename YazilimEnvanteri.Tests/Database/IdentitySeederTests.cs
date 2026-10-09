using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Data;
using YazilimEnvanteri.Models.Identity;

namespace YazilimEnvanteri.Tests.Database;

[Collection(PostgresCollection.Name)]
public class IdentitySeederTests(PostgresFixture db)
{
    [Fact]
    public async Task Roles_are_seeded_exactly_once()
    {
        db.App.CreateClient(); // boots the app, which seeds
        await IdentitySeeder.SeedAsync(db.App.Services);
        await IdentitySeeder.SeedAsync(db.App.Services);

        using var scope = db.App.Services.CreateScope();
        var roles = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Roles.Select(r => r.Name!).ToListAsync();
        Assert.Equal(AppRoles.All.Order(), roles.Order());
    }

    [Fact]
    public async Task Bootstrap_superadmin_is_created_once_from_configuration()
    {
        var email = $"admin{Guid.NewGuid():N}@example.com";

        // Two app starts with the same SeedAdmin settings.
        for (var i = 0; i < 2; i++)
        {
            await using var app = db.App.WithWebHostBuilder(b => b
                .UseSetting("SeedAdmin:Email", email)
                .UseSetting("SeedAdmin:Password", "Bootstrap!123"));
            app.CreateClient();
        }

        using var scope = db.App.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Equal(1, await users.Users.CountAsync(u => u.Email == email));
        var user = await users.FindByEmailAsync(email);
        Assert.True(await users.IsInRoleAsync(user!, AppRoles.SuperAdmin));
        Assert.True(await users.CheckPasswordAsync(user!, "Bootstrap!123"));
        Assert.NotEqual("Bootstrap!123", user!.PasswordHash);
    }

    [Fact]
    public async Task Staff_get_passwordless_accounts_once_with_role_by_specialist_status()
    {
        var tag = Guid.NewGuid().ToString("N");
        string devEmail = $"dev{tag}@example.com", staffEmail = $"staff{tag}@example.com", claimedEmail = $"kayitli{tag}@example.com";

        // An account someone already registered under a specialist's e-mail.
        using (var scope = db.App.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var existing = new ApplicationUser { UserName = claimedEmail, Email = claimedEmail };
            await users.CreateAsync(existing, "Test!12345");
            await users.AddToRoleAsync(existing, AppRoles.Observer);
        }

        int devId, staffId, claimedId, noEmailId;
        await using (var dataSource = db.CreateDataSourceWithoutForeignKeys())
        await using (var c = await dataSource.OpenConnectionAsync())
        {
            const string insert = """
                INSERT INTO "Personeller" ("BirimId", "Ad", "Soyad", "Email", "Telefon", "Gorev", "OlusturmaTarihi")
                VALUES (1, 'Ad', 'Soyad', @email, '', 'Görev', now()) RETURNING "Id";
                """;
            devId = await c.ExecuteScalarAsync<int>(insert, new { email = devEmail });
            staffId = await c.ExecuteScalarAsync<int>(insert, new { email = staffEmail });
            claimedId = await c.ExecuteScalarAsync<int>(insert, new { email = claimedEmail });
            noEmailId = await c.ExecuteScalarAsync<int>(insert, new { email = "" });
            await c.ExecuteAsync("""
                INSERT INTO "YazilimUzmanlari" ("BirimId", "PersonelId", "ProjeId", "SorumluFirma", "OlusturmaTarihi")
                VALUES (1, @devId, 0, 'Firma', now()), (1, @claimedId, 0, 'Firma', now());
                """, new { devId, claimedId });
        }

        await IdentitySeeder.SeedAsync(db.App.Services);
        await IdentitySeeder.SeedAsync(db.App.Services); // restart: nothing new

        using (var scope = db.App.Services.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var ids = new[] { devId, staffId, claimedId, noEmailId };
            var personeller = await sp.GetRequiredService<ApplicationDbContext>().Personeller
                .Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            async Task<ApplicationUser> Linked(int personelId, string email)
            {
                var user = await users.FindByEmailAsync(email);
                Assert.NotNull(user);
                Assert.Equal(user.Id, personeller[personelId].AppUserId);
                return user;
            }

            var dev = await Linked(devId, devEmail);
            Assert.Equal([AppRoles.Developer], await users.GetRolesAsync(dev));
            Assert.False(await users.HasPasswordAsync(dev));

            var staff = await Linked(staffId, staffEmail);
            Assert.Equal([AppRoles.Observer], await users.GetRolesAsync(staff));

            // Linked, but not promoted to Developer.
            var claimed = await Linked(claimedId, claimedEmail);
            Assert.Equal([AppRoles.Observer], await users.GetRolesAsync(claimed));

            Assert.Null(personeller[noEmailId].AppUserId);
            Assert.Equal(1, await users.Users.CountAsync(u => u.Email == devEmail));
        }
    }

    [Fact]
    public async Task Existing_account_with_the_seed_email_is_not_promoted()
    {
        var email = $"kayitli{Guid.NewGuid():N}@example.com";
        using (var scope = db.App.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { UserName = email, Email = email };
            await users.CreateAsync(user, "Test!12345");
            await users.AddToRoleAsync(user, AppRoles.Observer);
        }

        await using (var app = db.App.WithWebHostBuilder(b => b
            .UseSetting("SeedAdmin:Email", email)
            .UseSetting("SeedAdmin:Password", "Bootstrap!123")))
        {
            app.CreateClient();
        }

        using (var scope = db.App.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.Equal([AppRoles.Observer], await users.GetRolesAsync((await users.FindByEmailAsync(email))!));
        }
    }
}
