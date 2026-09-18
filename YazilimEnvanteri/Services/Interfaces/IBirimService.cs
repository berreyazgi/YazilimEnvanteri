using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Services.Interfaces
{
    public interface IBirimService
    {
        Task<IReadOnlyList<BirimEntity>> GetAllAsync();
        Task<BirimEntity?> GetByIdAsync(int id);
        Task<int> CreateAsync(BirimEntity entity);
        Task<bool> UpdateAsync(BirimEntity entity);
        Task<bool> DeleteAsync(int id);
    }
}
