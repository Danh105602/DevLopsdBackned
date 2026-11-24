
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using WebApplication1.Authorization;
using WebApplication1.Services;
using WebApplication1.DTOs;
using WebApplication1.DTOs.Common;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        // GET: api/users?pageNumber=1&pageSize=10
        // Chỉ Admin
        [HttpGet]
        [Authorize(Policy = Permissions.UserRead)]
        public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAll(
            [FromQuery] UserQueryParameters queryParams)
        {
            var result = await _service.GetAllAsync(queryParams);

            return Ok(result);
        }

        // GET: api/users/1
        // Chỉ Admin
        [HttpGet("{id:int}")]
        [Authorize(Policy = Permissions.UserRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetUser(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy user"
                });
            }

            return Ok(user);
        }

        // DELETE: api/users/1
        // Chỉ Admin
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Permissions.UserDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteUser(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy user"
                });
            }

            return Ok(new
            {
                message = "Xóa user thành công"
            });
        }

        // GET: api/users/me
        // Admin và Farmer đều được xem thông tin của chính mình
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                UserId = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value,

                Username = User.FindFirst(
                    ClaimTypes.Name)?.Value,

                Email = User.FindFirst(
                    ClaimTypes.Email)?.Value,

                Role = User.FindFirst(
                    ClaimTypes.Role)?.Value,

                IsAuthenticated = User.Identity?.IsAuthenticated
            });
        }

        // GET: api/users/policy-test
        // Chỉ Admin
        [HttpGet("policy-test")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult PolicyTest()
        {
            return Ok(new
            {
                message = "Bạn đã vượt qua Policy AdminOnly",
                role = User.FindFirst(
                    ClaimTypes.Role)?.Value
            });
        }
    }
}
