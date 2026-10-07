namespace YazilimEnvanteri.Models.ViewModels
{
    // Mirrors the client-side filter state in wwwroot/js/modules/projects.js so exports reflect
    // exactly what is on screen when the user clicks Excel/PDF. The list filters are multi-select:
    // values inside one list are OR-ed, different filters are AND-ed (e.g. ?durum=Analiz&durum=Test).
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
