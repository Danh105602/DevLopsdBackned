using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class FieldService : IFieldService
    {
        private readonly IFieldRepository _repository;

        public FieldService(IFieldRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Field>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Field?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Field> CreateAsync(Field field)
        {
            return await _repository.CreateAsync(field);
        }

        public async Task<Field?> UpdateAsync(int id, Field field)
        {
            return await _repository.UpdateAsync(id, field);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}