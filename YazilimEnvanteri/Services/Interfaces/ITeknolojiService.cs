using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Services.Interfaces
{
    public interface ITeknolojiService
    {
        Task<IReadOnlyList<TeknolojiEntity>> GetAllAsync();
        Task<TeknolojiEntity?> GetByIdAsync(int id);
        Task<int> CreateAsync(TeknolojiEntity entity);
        Task<bool> UpdateAsync(TeknolojiEntity entity);
        Task<bool> DeleteAsync(int id);
    }
}
