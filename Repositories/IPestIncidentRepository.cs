using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IPestIncidentRepository
    {
        Task<List<PestIncident>> GetAllAsync();
        Task<PestIncident?> GetByIdAsync(int id);
        Task<PestIncident> CreateAsync(PestIncident incident);
        Task<PestIncident?> UpdateAsync(int id, PestIncident incident);
        Task<bool> DeleteAsync(int id);
    }
}
