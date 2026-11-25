using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ICareScheduleService
    {
        Task<List<CareSchedule>> GetAllAsync();
        Task<CareSchedule?> GetByIdAsync(int id);
        Task<CareSchedule> CreateAsync(CareSchedule schedule);
        Task<CareSchedule?> UpdateAsync(int id, CareSchedule schedule);
        Task<bool> CompleteAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
