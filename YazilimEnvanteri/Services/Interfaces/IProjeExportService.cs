using YazilimEnvanteri.Models.ViewModels;

namespace YazilimEnvanteri.Services.Interfaces
{
    public interface IProjeExportService
    {
        byte[] ExportToExcel(IReadOnlyList<ProjeListItemViewModel> projeler);
        byte[] ExportToPdf(IReadOnlyList<ProjeListItemViewModel> projeler);
    }
}
