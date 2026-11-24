
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _service;

        public AuthController(IUserService service)
        {
            _service = service;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister dto)
        {
            return Ok(await _service.Register(dto));
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin dto)
        {
            var result = await _service.Login(dto);

            if (result == null)
                return Unauthorized("Invalid username or password.");

            return Ok(result);
        }

        // REFRESH TOKEN
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            RefreshTokenRequest dto)
        {
            var result = await _service.RefreshToken(dto);

            if (result == null)
                return Unauthorized("Refresh Token không hợp lệ hoặc đã hết hạn.");

            return Ok(result);
        }

        // LOGOUT
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            RefreshTokenRequest dto)
        {
            var result = await _service.Logout(dto);

            if (!result)
                return BadRequest("Refresh Token không hợp lệ hoặc đã bị thu hồi.");

            return Ok(new
            {
                message = "Logout thành công."
            });
        }
    }
}
