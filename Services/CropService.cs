using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class CropService : ICropService
    {
        private readonly ICropRepository _repository;

        public CropService(ICropRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Crop>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Crop?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Crop> CreateAsync(Crop crop)
        {
            return await _repository.CreateAsync(crop);
        }

        public async Task<Crop?> UpdateAsync(int id, Crop crop)
        {
            return await _repository.UpdateAsync(id, crop);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}