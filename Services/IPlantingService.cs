using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IPlantingService
    {
        Task<List<Planting>> GetAllAsync();
        Task<Planting?> GetByIdAsync(int id);
        Task<Planting> CreateAsync(Planting planting);
        Task<Planting?> UpdateAsync(int id, Planting planting);
        Task<bool> DeleteAsync(int id);
    }
}