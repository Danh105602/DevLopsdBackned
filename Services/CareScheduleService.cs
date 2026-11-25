using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class CareScheduleService : ICareScheduleService
    {
        private readonly ICareScheduleRepository _repository;

        public CareScheduleService(ICareScheduleRepository repository) => _repository = repository;

        public Task<List<CareSchedule>> GetAllAsync() => _repository.GetAllAsync();
        public Task<CareSchedule?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<CareSchedule> CreateAsync(CareSchedule schedule) => _repository.CreateAsync(schedule);
        public Task<CareSchedule?> UpdateAsync(int id, CareSchedule schedule) => _repository.UpdateAsync(id, schedule);
        public Task<bool> CompleteAsync(int id) => _repository.CompleteAsync(id);
        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
