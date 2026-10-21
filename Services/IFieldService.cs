using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IFieldService
    {
        Task<List<Field>> GetAllAsync();
        Task<Field?> GetByIdAsync(int id);
        Task<Field> CreateAsync(Field field);
        Task<Field?> UpdateAsync(int id, Field field);
        Task<bool> DeleteAsync(int id);
    }
}