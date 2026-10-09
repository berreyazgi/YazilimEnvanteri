namespace YazilimEnvanteri.Models.ViewModels
{
    public class ProjeListItemViewModel
    {
        public int Id { get; set; }
        public string ProjeKodu { get; set; } = string.Empty;
        public string ProjeAdi { get; set; } = string.Empty;
        public string ProjeHizmetAlani { get; set; } = string.Empty;
        public string ProjeAciklamasi { get; set; } = string.Empty;
        public bool ProjeAktifMi { get; set; }
        public string? Sunucu { get; set; }
        public string? WebsiteUrl { get; set; }
        public string ProjeDurum { get; set; } = string.Empty;
        public string ProjeKritiklik { get; set; } = string.Empty;

        // Raw foreign keys, so the inline Birim / Yazılım Uzmanı dropdowns can preselect the current value.
        public int BirimId { get; set; }
        public int YazilimUzmaniId { get; set; }

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
