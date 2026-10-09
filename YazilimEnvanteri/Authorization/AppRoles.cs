namespace YazilimEnvanteri.Authorization
{
    // The only roles the application knows. Anything else coming from a request is rejected
    // (see RoleManagementController) and IdentitySeeder creates exactly these.
    public static class AppRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Developer = "Developer";
        public const string Observer = "Observer";

        public static readonly IReadOnlyList<string> All = [SuperAdmin, Admin, Developer, Observer];

        // Ordinal: "superadmin" is not a valid role name.
        public static bool IsValid(string? role) => role is not null && All.Contains(role, StringComparer.Ordinal);
    }
}
