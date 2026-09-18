namespace YazilimEnvanteri.Models.ViewModels
{
    // Dashboard summary statistics for the Home page. Kept separate from ProjeListItemViewModel
    // since it represents aggregate counts, not project rows. Designed to grow with more
    // breakdowns (e.g. per-Birim, per-Teknoloji, per-Kritiklik) once Chart.js is introduced,
    // without needing to touch the page's existing metric cards.
    public class DashboardViewModel
    {
        public int ToplamProje { get; set; }
        public int YayindakiProje { get; set; }
        public int GelistirmedekiProje { get; set; }
        public int TestIncelemeProje { get; set; }
    }
}
