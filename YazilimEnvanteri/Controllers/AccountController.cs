using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YazilimEnvanteri.Authorization;
using YazilimEnvanteri.Models.Identity;
using YazilimEnvanteri.Models.ViewModels;

namespace YazilimEnvanteri.Controllers
{
    public class AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger) : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            // UserName == Email for every account (see Register / IdentitySeeder).
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded) return RedirectToLocal(returnUrl);

            ModelState.AddModelError(string.Empty, result.IsLockedOut
                ? "Çok fazla hatalı deneme yapıldı. Hesap geçici olarak kilitlendi, lütfen daha sonra tekrar deneyin."
                : "E-posta veya şifre hatalı.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, AdSoyad = model.AdSoyad.Trim() };
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
                result = await userManager.AddToRoleAsync(user, AppRoles.Observer);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            logger.LogInformation("Yeni kullanıcı kaydoldu: {Email}", model.Email);
            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // The password change rotates the security stamp; re-issue this session's cookie so
            // only *other* sessions get signed out.
            await signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Şifreniz değiştirildi.";
            return RedirectToAction(nameof(ChangePassword));
        }

        // Shows no protected data - just says the action isn't allowed for this role.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // Only local URLs, so ?returnUrl= can't bounce a fresh login to another site.
        private IActionResult RedirectToLocal(string? returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
    }
}
