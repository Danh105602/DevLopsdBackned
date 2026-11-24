
using WebApplication1.DTOs;
using WebApplication1.DTOs.Common;

namespace WebApplication1.Services
{
    public interface IUserService
    {
        Task<UserResponse> Register(UserRegister dto);

        Task<AuthResponse?> Login(UserLogin dto);

        Task<AuthResponse?> RefreshToken(RefreshTokenRequest dto);

        Task<bool> Logout(RefreshTokenRequest dto);

        Task<List<UserResponse>> GetUsers();

        Task<UserResponse?> GetUser(int id);

        Task<bool> DeleteUser(int id);

        Task<PagedResult<UserResponseDto>> GetAllAsync(
            UserQueryParameters queryParams);
    }
}
