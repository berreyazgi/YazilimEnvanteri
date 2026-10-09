namespace YazilimEnvanteri.Models.ViewModels
{
    // Kullanıcı Yönetimi row - only what the page shows, never the Identity user itself
    // (no password hash, security stamp, tokens...).
    public class UserRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();

        // False for accounts created by the Personeller sync until a temporary password is generated.
        public bool HasPassword { get; set; }
    }
}
