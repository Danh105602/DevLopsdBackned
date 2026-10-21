using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();

        Task<User?> GetByIdAsync(int id);

        Task<User?> GetUsernameAsync(string username);

        Task<User?> GetByEmailAsync(string email);

        Task AddAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(User user);
    }
}