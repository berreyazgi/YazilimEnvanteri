using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Services.Interfaces
{
    public interface IYazilimUzmaniService
    {
        Task<IReadOnlyList<YazilimUzmaniEntity>> GetAllAsync();
        Task<YazilimUzmaniEntity?> GetByIdAsync(int id);
        Task<int> CreateAsync(YazilimUzmaniEntity entity);
        Task<bool> UpdateAsync(YazilimUzmaniEntity entity);
        Task<bool> DeleteAsync(int id);
    }
}
