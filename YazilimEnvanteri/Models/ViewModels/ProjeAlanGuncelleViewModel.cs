namespace YazilimEnvanteri.Models.ViewModels
{
    // POST /Proje/UpdateField body. `Field` is only matched against ProjeController's allowlist -
    // never used to look up a property by name.
    public class ProjeAlanGuncelleViewModel
    {
        public int Id { get; set; }
        public string Field { get; set; } = string.Empty;
        public string? Value { get; set; }
    }
}
