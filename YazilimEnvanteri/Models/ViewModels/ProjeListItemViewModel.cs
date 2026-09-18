namespace YazilimEnvanteri.Models.ViewModels
{
    public class ProjeListItemViewModel
    {
        public int Id { get; set; }
        public int ProjeKodu { get; set; }
        public string ProjeAdi { get; set; } = string.Empty;
        public string ProjeHizmetAlani { get; set; } = string.Empty;
        public string ProjeAciklamasi { get; set; } = string.Empty;
        public bool ProjeAktifMi { get; set; }
        public string? Sunucu { get; set; }
        public string? WebsiteUrl { get; set; }
        public string ProjeDurum { get; set; } = string.Empty;
        public string ProjeKritiklik { get; set; } = string.Empty;

        public string? Birim { get; set; }
        public string? UstBirim { get; set; }
        public string? AltBirim { get; set; }

        public string? YazilimUzmaniAdSoyad { get; set; }
        public string? YazilimUzmaniKullaniciAdi { get; set; }
        public string? YazilimUzmaniGorev { get; set; }
        public string? YazilimUzmaniEposta { get; set; }
        public string? YazilimUzmaniTelefon { get; set; }
        public string? YazilimUzmaniSorumluFirma { get; set; }

        public string? BackendTeknoloji { get; set; }
        public string? FrontendTeknoloji { get; set; }
        public string? Veritabani { get; set; }
        public bool? EntegrasyonDurum { get; set; }
        public bool? EimzaDurum { get; set; }
    }
}
