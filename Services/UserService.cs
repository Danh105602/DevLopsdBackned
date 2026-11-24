
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

using WebApplication1.Authorization;
using WebApplication1.Exceptions;
using WebApplication1.DTOs;
using WebApplication1.DTOs.Common;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IConfiguration _configuration;

        private const int AccessTokenMinutes = 15;
        private const int RefreshTokenDays = 7;

        public UserService(
            IUserRepository repository,
            IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        // =========================
        // 1. REGISTER
        // =========================
        public async Task<UserResponse> Register(UserRegister dto)
        {
            var existingUsername =
                await _repository.GetUsernameAsync(dto.Username);

            if (existingUsername != null)
                throw new ConflictException("Username đã tồn tại");

            var existingEmail =
                await _repository.GetByEmailAsync(dto.Email);

            if (existingEmail != null)
                throw new ConflictException("Email đã tồn tại");

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
        // =========================
        // 2. LOGIN
        // =========================
        public async Task<AuthResponse?> Login(UserLogin dto)
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

            return await CreateAuthResponse(user);
        }

        // =========================
        // 3. REFRESH TOKEN
        // =========================
        public async Task<AuthResponse?> RefreshToken(
            RefreshTokenRequest dto)
        {
            var tokenHash = HashToken(dto.RefreshToken);

            var storedToken =
                await _repository.GetRefreshTokenByHashAsync(tokenHash);

            if (storedToken == null)
                return null;

            if (storedToken.RevokedAt != null ||
                storedToken.ExpiresAt <= DateTime.UtcNow)
                return null;

            var user =
                await _repository.GetByIdAsync(storedToken.UserId);

            if (user == null || !user.IsActive)
                return null;

            // Thu hồi token cũ
            storedToken.RevokedAt = DateTime.UtcNow;

            // Tạo cặp token mới
            var response = await CreateAuthResponse(user);

            // Ghi nhận token thay thế
            storedToken.ReplacedByTokenHash =
                HashToken(response.RefreshToken);

            await _repository.UpdateRefreshTokenAsync(storedToken);

            return response;
        }

        // =========================
        // 4. LOGOUT
        // =========================
        public async Task<bool> Logout(RefreshTokenRequest dto)
        {
            var tokenHash = HashToken(dto.RefreshToken);

            var storedToken =
                await _repository.GetRefreshTokenByHashAsync(tokenHash);

            if (storedToken == null || storedToken.RevokedAt != null)
                return false;

            storedToken.RevokedAt = DateTime.UtcNow;

            await _repository.UpdateRefreshTokenAsync(storedToken);

            return true;
        }

        // =========================
        // 5. CREATE AUTH RESPONSE
        // =========================
        private async Task<AuthResponse> CreateAuthResponse(User user)
        {
            var now = DateTime.UtcNow;

            var refreshTokenExpiresAt =
                now.AddDays(RefreshTokenDays);

            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(refreshToken),
                CreatedAt = now,
                ExpiresAt = refreshTokenExpiresAt
            };

            await _repository.AddRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponse
            {
                AccessToken = GenerateToken(user),
                RefreshToken = refreshToken,
                AccessTokenExpiresAt =
                    now.AddMinutes(AccessTokenMinutes),
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        // =========================
        // 6. GENERATE ACCESS TOKEN
        // =========================
        private string GenerateToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("JWT Key chưa được cấu hình");

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
                throw new Exception(
                    "JWT Issuer hoặc Audience chưa được cấu hình");

            // Claims cơ bản
            var claims = new List<Claim>
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

            // Thêm Permission theo Role
            claims.AddRange(
                RolePermissions.GetPermissions(user.Role)
                    .Select(permission =>
                        new Claim("permission", permission))
            );

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    AccessTokenMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        // =========================
        // 7. GENERATE REFRESH TOKEN
        // =========================
        private string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }

        // =========================
        // 8. HASH REFRESH TOKEN
        // =========================
        private string HashToken(string token)
        {
            var bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(bytes);
        }

        // =========================
        // 9. GET ALL USERS
        // =========================
        public async Task<List<UserResponse>> GetUsers()
        {
            var users = await _repository.GetAllAsync();

            return users.Select(MapToResponse).ToList();
        }

        // =========================
        // 10. GET USER BY ID
        // =========================
        public async Task<UserResponse?> GetUser(int id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                return null;

            return MapToResponse(user);
        }

        // =========================
        // 11. DELETE USER
        // =========================
        public async Task<bool> DeleteUser(int id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                return false;

            await _repository.DeleteAsync(user);

            return true;
        }

        // =========================
        // 12. MAP USER RESPONSE
        // =========================
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

        // =========================
        // 13. PAGINATION
        // =========================
        public async Task<PagedResult<UserResponseDto>> GetAllAsync(
            UserQueryParameters queryParams)
        {
            var pagedUsers =
                await _repository.GetAllAsync(queryParams);

            var mappedItems = pagedUsers.Items
                .Select(MapToResponseDto)
                .ToList();

            return new PagedResult<UserResponseDto>(
                mappedItems,
                pagedUsers.TotalCount,
                pagedUsers.PageNumber,
                pagedUsers.PageSize
            );
        }

        private UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
