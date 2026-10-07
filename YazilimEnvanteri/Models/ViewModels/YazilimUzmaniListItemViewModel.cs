namespace YazilimEnvanteri.Models.ViewModels
{
    // YazilimUzmani + its Personel/Birim, flattened for the JSON endpoints and the "Yazılım Uzmanı"
    // dropdown - the identity fields come from Personeller, not from YazilimUzmanlari.
    public class YazilimUzmaniListItemViewModel
    {
        public int Id { get; set; }
        public int PersonelId { get; set; }
        public int ProjeId { get; set; }
        public int BirimId { get; set; }
        public string? Birim { get; set; }
        public string SorumluFirma { get; set; } = string.Empty;

        public string? KullaniciAdi { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string AdSoyad => $"{Ad} {Soyad}".Trim();
        public string Email { get; set; } = string.Empty;
        public string Gorev { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
    }
}
