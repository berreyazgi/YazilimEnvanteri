using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Data;
using YazilimEnvanteri.Models.Identity;
using YazilimEnvanteri.Models.ViewModels;

namespace YazilimEnvanteri.Controllers
{
    // Kullanıcı Yönetimi - SuperAdmin only, for every action (including the page itself).
    [Authorize(Policy = AuthorizationPolicies.CanManageUsers)]
    public class RoleManagementController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext db) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index() => View(await GetRowsAsync());

        // Sets a fresh random password and shows it in this response only - it is never stored or
        // logged in plain text, so if it's lost the SuperAdmin simply generates another. Also
        // lifts any lockout, and (via the security stamp) signs the user out elsewhere.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = string.IsNullOrWhiteSpace(userId) ? null : await userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var password = GenerateTemporaryPassword();
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
                return Fail("Şifre oluşturulamadı: " + string.Join(" ", result.Errors.Select(e => e.Description)));
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);

            ViewData["TempPasswordEmail"] = user.Email;
            ViewData["TempPassword"] = password;
            Response.Headers.CacheControl = "no-store";
            return View(nameof(Index), await GetRowsAsync());
        }

        private async Task<List<UserRoleViewModel>> GetRowsAsync()
        {
            var users = await db.Users
                .OrderBy(u => u.Email)
                .Select(u => new UserRoleViewModel { Id = u.Id, AdSoyad = u.AdSoyad, Email = u.Email ?? string.Empty, HasPassword = u.PasswordHash != null })
                .ToListAsync();

            var userRoles = (await (from ur in db.UserRoles
                                    join r in db.Roles on ur.RoleId equals r.Id
                                    select new { ur.UserId, r.Name }).ToListAsync())
                .ToLookup(x => x.UserId, x => x.Name);

            // Canonical order (SuperAdmin, Admin, Developer, Observer), app roles only.
            foreach (var user in users)
                user.Roles = AppRoles.All.Where(role => userRoles[user.Id].Contains(role)).ToList();

            return users;
        }

        // 16 chars from all four classes Identity's default password policy requires.
        private static string GenerateTemporaryPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ", lower = "abcdefghijkmnpqrstuvwxyz", digits = "23456789", symbols = "!@#$%*?-_";
            var chars = (RandomNumberGenerator.GetString(upper, 1) + RandomNumberGenerator.GetString(lower, 1)
                + RandomNumberGenerator.GetString(digits, 1) + RandomNumberGenerator.GetString(symbols, 1)
                + RandomNumberGenerator.GetString(upper + lower + digits + symbols, 12)).ToCharArray();
            RandomNumberGenerator.Shuffle(chars.AsSpan());
            return new string(chars);
        }

        // Replaces the user's application roles with exactly `roles`. Everything posted is
        // re-validated here - the checkboxes on the page are not trusted.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, List<string>? roles)
        {
            var user = string.IsNullOrWhiteSpace(userId) ? null : await userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var requested = (roles ?? []).Distinct(StringComparer.Ordinal).ToList();
            if (requested.Count == 0)
                return Fail("En az bir rol seçilmelidir.");
            if (!requested.All(AppRoles.IsValid))
                return Fail("Geçersiz rol seçimi. Yalnızca SuperAdmin, Admin, Developer ve Observer atanabilir.");

            var current = (await userManager.GetRolesAsync(user)).Where(AppRoles.IsValid).ToList();
            var toRemove = current.Except(requested).ToList();
            var toAdd = requested.Except(current).ToList();

            // ponytail: check-then-act, two SuperAdmins demoting each other at the same instant could
            // both pass; use a Serializable transaction here if that ever becomes realistic.
            if (toRemove.Contains(AppRoles.SuperAdmin)
                && (await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin)).Count <= 1)
            {
                return Fail("Sistemdeki son SuperAdmin'in SuperAdmin rolü kaldırılamaz. Önce başka bir kullanıcıyı SuperAdmin yapın.");
            }

            // Remove + add commit together, so a failure can't leave the user with no role.
            await using var transaction = await db.Database.BeginTransactionAsync();
            var result = toRemove.Count > 0 ? await userManager.RemoveFromRolesAsync(user, toRemove) : IdentityResult.Success;
            if (result.Succeeded && toAdd.Count > 0)
                result = await userManager.AddToRolesAsync(user, toAdd);
            if (!result.Succeeded)
                return Fail("Roller güncellenemedi: " + string.Join(" ", result.Errors.Select(e => e.Description)));
            await transaction.CommitAsync();

            // Own roles changed: re-issue this session's cookie now instead of waiting for the
            // security stamp re-validation (other users pick it up within a minute, see Program.cs).
            if (user.Id == userManager.GetUserId(User))
                await signInManager.RefreshSignInAsync(user);

            TempData["Success"] = $"{user.Email} kullanıcısının rolleri güncellendi: {string.Join(", ", AppRoles.All.Where(requested.Contains))}.";
            return RedirectToAction(nameof(Index));
        }

        private RedirectToActionResult Fail(string message)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}
