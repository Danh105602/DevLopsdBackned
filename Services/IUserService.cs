using WebApplication1.DTOs;

namespace WebApplication1.Services
{
    public interface IUserService
    {
        Task<UserResponse> Register(UserRegister dto);

        Task<string?> Login(UserLogin dto);

        Task<List<UserResponse>> GetUsers();

        Task<UserResponse?> GetUser(int id);

        Task<bool> DeleteUser(int id);
    }
}