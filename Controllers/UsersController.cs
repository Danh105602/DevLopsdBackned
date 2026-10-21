using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using BCrypt.Net;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        // Fake Data
        private static readonly List<UserResponse> users = new()
        {
            new UserResponse
            {
                Id = 1,
                Username = "admin",
                Email = "admin@gmail.com",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            },

            new UserResponse
            {
                Id = 2,
                Username = "staff01",
                Email = "staff01@gmail.com",
                Role = "Staff",
                CreatedAt = DateTime.UtcNow
            },

            new UserResponse
            {
                Id = 3,
                Username = "customer01",
                Email = "customer01@gmail.com",
                Role = "Customer",
                CreatedAt = DateTime.UtcNow
            },

            new UserResponse
            {
                Id = 4,
                Username = "customer02",
                Email = "customer02@gmail.com",
                Role = "Customer",
                CreatedAt = DateTime.UtcNow
            }
        };

        // =========================================
        // GET: api/users
        // Lấy tất cả user
        // =========================================
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(users);
        }

        // =========================================
        // GET: api/users/1
        // Lấy user theo ID
        // =========================================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy user"
                });
            }

            return Ok(user);
        }

        // =========================================
        // POST: api/users
        // Thêm user
        // =========================================




        [HttpPost]
        public IActionResult Create(UserCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Username, Email và Password không được để trống"
                });
            }

            if (users.Any(x =>
                x.Username.Equals(
                    request.Username,
                    StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { message = "Username đã tồn tại" });
            }

            if (users.Any(x =>
                x.Email.Equals(
                    request.Email,
                    StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { message = "Email đã tồn tại" });
            }

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new UserResponse
            {
                Id = users.Count == 0 ? 1 : users.Max(x => x.Id) + 1,
                Username = request.Username,
                Email = request.Email,
                Role = string.IsNullOrWhiteSpace(request.Role)
                    ? "Customer"
                    : request.Role,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            users.Add(newUser);

            return CreatedAtAction(
                nameof(GetById),
                new { id = newUser.Id },
                new
                {
                    newUser.Id,
                    newUser.Username,
                    newUser.Email,
                    newUser.Role,
                    newUser.CreatedAt
                });
        }

        // =========================================
        // DELETE: api/users/1
        // Xóa user
        // =========================================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy user"
                });
            }

            users.Remove(user);

            return Ok(new
            {
                message = "Xóa user thành công"
            });
        }
    }

    // DTO dùng để thêm User
    public class UserCreateRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "Customer";

        public string Password { get; set; } = string.Empty;
    }

    // DTO dùng để cập nhật User
    public class UserUpdateRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "Customer";
    }
}