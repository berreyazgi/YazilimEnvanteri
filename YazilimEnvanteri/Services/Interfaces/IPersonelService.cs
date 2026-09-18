using YazilimEnvanteri.Models.Entities;

namespace YazilimEnvanteri.Services.Interfaces
{
    public interface IPersonelService
    {
        Task<IReadOnlyList<PersonelEntity>> GetAllAsync();
        Task<PersonelEntity?> GetByIdAsync(int id);
        Task<int> CreateAsync(PersonelEntity entity);
        Task<bool> UpdateAsync(PersonelEntity entity);
        Task<bool> DeleteAsync(int id);
    }
}
