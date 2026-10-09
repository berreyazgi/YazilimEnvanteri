using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Models.Identity;

namespace YazilimEnvanteri.Data
{
    // Runs on every startup and is idempotent: creates whichever of the four roles are missing,
    // when SeedAdmin:Email / SeedAdmin:Password are configured (env: SeedAdmin__Email /
    // SeedAdmin__Password, or user-secrets locally) the first SuperAdmin account, and a login
    // account for every Personeller row that doesn't have one yet.
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(IdentitySeeder));

            try
            {
                // Migrations are applied by hand (dotnet ef database update); don't touch a schema
                // that isn't there yet - the app still starts and /health reports the database.
                var pending = await sp.GetRequiredService<ApplicationDbContext>().Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    logger.LogError("Bekleyen migration'lar var ({Migrations}); rol/SuperAdmin tohumlama atlandı. 'dotnet ef database update' çalıştırın.",
                        string.Join(", ", pending));
                    return;
                }

                await SeedRolesAsync(sp.GetRequiredService<RoleManager<IdentityRole>>(), logger);
                await SeedSuperAdminAsync(sp.GetRequiredService<UserManager<ApplicationUser>>(), sp.GetRequiredService<IConfiguration>(), logger);
                await SyncStaffUsersAsync(sp.GetRequiredService<ApplicationDbContext>(), sp.GetRequiredService<UserManager<ApplicationUser>>(), logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Kimlik verileri tohumlanırken hata oluştu.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            foreach (var role in AppRoles.All)
            {
                if (await roleManager.RoleExistsAsync(role)) continue;

                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"'{role}' rolü oluşturulamadı: {Describe(result)}");
                logger.LogInformation("'{Role}' rolü oluşturuldu.", role);
            }
        }

        private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration config, ILogger logger)
        {
            var email = config["SeedAdmin:Email"]?.Trim();
            var password = config["SeedAdmin:Password"];
            if (string.IsNullOrEmpty(email)) return;

            var user = await userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                // Never promote an account that already exists: with public registration anyone
                // could have signed up under this address and chosen their own password.
                if (!await userManager.IsInRoleAsync(user, AppRoles.SuperAdmin))
                    logger.LogWarning("SeedAdmin e-postası ({Email}) mevcut bir kullanıcıya ait ve SuperAdmin değil; yükseltilmedi.", email);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                logger.LogWarning("SeedAdmin:Email ayarlı ama SeedAdmin:Password boş; ilk SuperAdmin oluşturulmadı.");
                return;
            }

            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, AdSoyad = "Sistem Yöneticisi" };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                result = await userManager.AddToRoleAsync(user, AppRoles.SuperAdmin);
            if (!result.Succeeded)
                throw new InvalidOperationException($"İlk SuperAdmin oluşturulamadı: {Describe(result)}");

            logger.LogInformation("İlk SuperAdmin oluşturuldu: {Email}", email);
        }

        // Personeller -> AspNetUsers. Only rows with no AppUserId are touched, so restarts are no-ops.
        // New accounts get no password (nobody can sign in with them) until a SuperAdmin generates
        // a one-time temporary password on Kullanıcı Yönetimi. Role: Developer if the person is a
        // YazilimUzmani, otherwise Observer.
        private static async Task SyncStaffUsersAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            var unlinked = await db.Personeller.Where(p => p.AppUserId == null).ToListAsync();
            if (unlinked.Count == 0) return;

            var developerIds = (await db.YazilimUzmanlari.Select(y => y.PersonelId).ToListAsync()).ToHashSet();
            var emailCheck = new EmailAddressAttribute();

            foreach (var personel in unlinked)
            {
                var email = personel.Email.Trim();
                if (email.Length == 0 || !emailCheck.IsValid(email))
                {
                    logger.LogWarning("Personel {Id} için geçerli e-posta yok; giriş hesabı oluşturulmadı.", personel.Id);
                    continue;
                }

                // Account creation + role + link commit together (UserManager shares this DbContext).
                await using var transaction = await db.Database.BeginTransactionAsync();

                var user = await userManager.FindByEmailAsync(email);
                if (user is null)
                {
                    user = new ApplicationUser { UserName = email, Email = email, AdSoyad = $"{personel.Ad} {personel.Soyad}".Trim() };
                    var role = developerIds.Contains(personel.Id) ? AppRoles.Developer : AppRoles.Observer;
                    var result = await userManager.CreateAsync(user);
                    if (result.Succeeded)
                        result = await userManager.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        logger.LogWarning("Personel {Id} ({Email}) için hesap oluşturulamadı: {Errors}", personel.Id, email, Describe(result));
                        continue;
                    }
                    logger.LogInformation("Personel {Id} için {Role} hesabı oluşturuldu: {Email}", personel.Id, role, email);
                }
                else if (await db.Personeller.AnyAsync(p => p.AppUserId == user.Id))
                {
                    logger.LogWarning("{Email} başka bir personele zaten bağlı; Personel {Id} bağlanmadı (yinelenen e-posta).", email, personel.Id);
                    continue;
                }
                // else: an account with this e-mail already exists (e.g. self-registered) - link it but
                // leave its roles alone; registration has no e-mail confirmation, so the address alone
                // doesn't prove it's really this person.

                personel.AppUserId = user.Id;
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        private static string Describe(IdentityResult result) => string.Join(" ", result.Errors.Select(e => e.Description));
    }
}
