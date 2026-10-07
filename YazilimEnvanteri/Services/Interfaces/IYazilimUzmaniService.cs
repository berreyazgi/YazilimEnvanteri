using YazilimEnvanteri.Models.Entities;
using YazilimEnvanteri.Models.ViewModels;

namespace YazilimEnvanteri.Services.Interfaces
{
    public interface IYazilimUzmaniService
    {
        Task<IReadOnlyList<YazilimUzmaniEntity>> GetAllAsync();
        Task<YazilimUzmaniEntity?> GetByIdAsync(int id);
        Task<int> CreateAsync(YazilimUzmaniEntity entity);
        Task<bool> UpdateAsync(YazilimUzmaniEntity entity);
        Task<bool> DeleteAsync(int id);

        // Joined with Personeller/Birimler so callers get the specialist's name, e-mail etc.
        // without YazilimUzmanlari having to duplicate them.
        Task<IReadOnlyList<YazilimUzmaniListItemViewModel>> GetListAsync();
        Task<YazilimUzmaniListItemViewModel?> GetDetailAsync(int id);
    }
}
