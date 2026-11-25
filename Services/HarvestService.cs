
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class HarvestService : IHarvestService
    {
        private readonly IHarvestRepository _repository;

        public HarvestService(IHarvestRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Harvest>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Harvest?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Harvest> CreateAsync(Harvest harvest)
        {
            return await _repository.CreateAsync(harvest);
        }

        public async Task<Harvest?> UpdateAsync(int id, Harvest harvest)
        {
            return await _repository.UpdateAsync(id, harvest);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
