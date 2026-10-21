using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public interface IFarmService
    {
        Task<List<Farm>> GetAllAsync();
        Task<Farm?> GetByIdAsync(int id);
        Task<Farm> CreateAsync(Farm farm);
        Task<Farm?> UpdateAsync(int id, Farm farm);
        Task<bool> DeleteAsync(int id);
    }

    public class FarmService : IFarmService
    {
        private readonly IFarmRepository _repository;

        public FarmService(IFarmRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Farm>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Farm?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Farm> CreateAsync(Farm farm)
        {
            return await _repository.CreateAsync(farm);
        }

        public async Task<Farm?> UpdateAsync(int id, Farm farm)
        {
            return await _repository.UpdateAsync(id, farm);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}