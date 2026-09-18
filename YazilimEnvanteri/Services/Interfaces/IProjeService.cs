using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.ViewModels;

namespace YazilimEnvanteri.Services.Interfaces
{
    public interface IProjeService
    {
        Task<IReadOnlyList<ProjeEntity>> GetAllAsync();
        Task<ProjeEntity?> GetByIdAsync(int id);
        Task<int> CreateAsync(ProjeEntity entity);
        Task<bool> UpdateAsync(ProjeEntity entity);
        Task<bool> DeleteAsync(int id);

        // Denormalized dashboard/detail projections - a single joined query instead of the
        // separate per-table lookups the old EF-based generic repository used to do.
        Task<IReadOnlyList<ProjeListItemViewModel>> GetProjeListAsync();
        Task<ProjeListItemViewModel?> GetProjeDetailAsync(int id);

        // Home dashboard summary counts, grouped by ProjeDurum in SQL rather than fetching every
        // project's full joined row just to count statuses.
        Task<DashboardViewModel> GetDashboardSummaryAsync();
    }
}
