using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IConfiguration _configuration;

        public UserService(
            IUserRepository repository,
            IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<UserResponse> Register(UserRegister dto)
        {
            var existingUsername =
                await _repository.GetUsernameAsync(dto.Username);

            if (existingUsername != null)
                throw new Exception("Username đã tồn tại");

            var existingEmail =
                await _repository.GetByEmailAsync(dto.Email);

            if (existingEmail != null)
                throw new Exception("Email đã tồn tại");

            using var hmac = new HMACSHA512();

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt = hmac.Key,
                Role = "Farmer",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.AddAsync(user);

            return MapToResponse(user);
        }

        public async Task<string?> Login(UserLogin dto)
        {
            var user =
                await _repository.GetUsernameAsync(dto.Username);

            if (user == null)
                return null;

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(dto.Password));

            if (!computedHash.SequenceEqual(user.PasswordHash))
                return null;

            if (!user.IsActive)
                return null;

            return GenerateToken(user);
        }

        public async Task<List<UserResponse>> GetUsers()
        {
            var users = await _repository.GetAllAsync();

            return users.Select(MapToResponse).ToList();
        }

        public async Task<UserResponse?> GetUser(int id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                return null;

            return MapToResponse(user);
        }

        public async Task<bool> DeleteUser(int id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                return false;

            await _repository.DeleteAsync(user);

            return true;
        }

        private UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        private string GenerateToken(User user)
        {
            var key = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(key))
                throw new Exception("JWT Key chưa được cấu hình");

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}