
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IHarvestRepository
    {
        Task<List<Harvest>> GetAllAsync();

        Task<Harvest?> GetByIdAsync(int id);

        Task<Harvest> CreateAsync(Harvest harvest);

        Task<Harvest?> UpdateAsync(int id, Harvest harvest);

        Task<bool> DeleteAsync(int id);
    }
}
