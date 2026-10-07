namespace YazilimEnvanteri.Models.ViewModels
{
       public class ProjeExportFilterViewModel
    {
        public string? Search { get; set; }
        public List<string> HizmetAlani { get; set; } = new();
        public List<string> Birim { get; set; } = new();
        public List<string> Durum { get; set; } = new();
        public List<string> Kritiklik { get; set; } = new();
        public bool SadeceAktif { get; set; }
        public bool SadeceWebAdresiOlan { get; set; }
    }
}
