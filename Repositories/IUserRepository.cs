using WebApplication1.Models;
using WebApplication1.DTOs.Common;

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

        Task<PagedResult<User>> GetAllAsync(UserQueryParameters queryParams);

        // REFRESH TOKEN
        Task AddRefreshTokenAsync(RefreshToken refreshToken);

        Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash);

        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    }
}