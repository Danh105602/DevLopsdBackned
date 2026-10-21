using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class PlantingService : IPlantingService
    {
        private readonly IPlantingRepository _repository;

        public PlantingService(IPlantingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Planting>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Planting?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Planting> CreateAsync(Planting planting)
        {
            return await _repository.CreateAsync(planting);
        }

        public async Task<Planting?> UpdateAsync(int id, Planting planting)
        {
            return await _repository.UpdateAsync(id, planting);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}