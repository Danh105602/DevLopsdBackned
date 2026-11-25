using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface ICareScheduleRepository
    {
        Task<List<CareSchedule>> GetAllAsync();
        Task<CareSchedule?> GetByIdAsync(int id);
        Task<CareSchedule> CreateAsync(CareSchedule schedule);
        Task<CareSchedule?> UpdateAsync(int id, CareSchedule schedule);
        Task<bool> CompleteAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
