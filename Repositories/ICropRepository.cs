using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface ICropRepository
    {
        Task<List<Crop>> GetAllAsync();
        Task<Crop?> GetByIdAsync(int id);
        Task<Crop> CreateAsync(Crop crop);
        Task<Crop?> UpdateAsync(int id, Crop crop);
        Task<bool> DeleteAsync(int id);
    }
}