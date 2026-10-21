using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IPlantingRepository
    {
        Task<List<Planting>> GetAllAsync();
        Task<Planting?> GetByIdAsync(int id);
        Task<Planting> CreateAsync(Planting planting);
        Task<Planting?> UpdateAsync(int id, Planting planting);
        Task<bool> DeleteAsync(int id);
    }
}