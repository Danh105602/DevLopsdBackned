
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimsController : ControllerBase
    {
        // GET: api/Claims/me
        [HttpGet("me")]
        public IActionResult GetMyClaims()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            return Ok(new
            {
                Message = "JWT Claims hợp lệ",
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Username = User.Identity?.Name,
                Claims = claims
            });
        }

        // GET: api/Claims/admin
        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AdminOnly()
        {
            return Ok(new
            {
                Message = "Bạn có quyền Admin",
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        // GET: api/Claims/farmer-or-admin
        [HttpGet("farmer-or-admin")]
        [Authorize(Policy = "FarmerOrAdmin")]
        public IActionResult FarmerOrAdmin()
        {
            return Ok(new
            {
                Message = "Bạn có quyền Farmer hoặc Admin",
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }
    }
}
