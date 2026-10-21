using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IFieldRepository
    {
        Task<List<Field>> GetAllAsync();
        Task<Field?> GetByIdAsync(int id);
        Task<Field> CreateAsync(Field field);
        Task<Field?> UpdateAsync(int id, Field field);
        Task<bool> DeleteAsync(int id);
    }
}