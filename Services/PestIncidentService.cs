using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class PestIncidentService : IPestIncidentService
    {
        private readonly IPestIncidentRepository _repository;

        public PestIncidentService(IPestIncidentRepository repository) => _repository = repository;

        public Task<List<PestIncident>> GetAllAsync() => _repository.GetAllAsync();
        public Task<PestIncident?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<PestIncident> CreateAsync(PestIncident incident) => _repository.CreateAsync(incident);
        public Task<PestIncident?> UpdateAsync(int id, PestIncident incident) => _repository.UpdateAsync(id, incident);
        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
