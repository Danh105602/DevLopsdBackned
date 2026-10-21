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

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister dto)
        {
            return Ok(await _service.Register(dto));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin dto)
        {
            var token = await _service.Login(dto);

            if (token == null)
                return Unauthorized(
                    "Invalid username or password.");

            return Ok(new { token });
        }
    }
}