using Microsoft.AspNetCore.Authorization;

namespace YazilimEnvanteri.Authorization
{
    public static class AuthorizationPolicies
    {
        public const string CanViewProjects = nameof(CanViewProjects);
        public const string CanManageProjects = nameof(CanManageProjects);
        public const string CanDeleteProjects = nameof(CanDeleteProjects);
        public const string CanManageUsers = nameof(CanManageUsers);

        public static void AddAppPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(CanViewProjects, p => p.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.Developer, AppRoles.Observer));
            options.AddPolicy(CanManageProjects, p => p.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.Developer));
            options.AddPolicy(CanDeleteProjects, p => p.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin));
            options.AddPolicy(CanManageUsers, p => p.RequireRole(AppRoles.SuperAdmin));

            // Every endpoint needs a signed-in user unless it opts out with [AllowAnonymous].
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        }
    }
}
