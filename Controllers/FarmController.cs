using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication1.Authorization;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.DTOs.Farm;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmController : ControllerBase
    {
        private readonly IFarmService _service;

        public FarmController(IFarmService service)
        {
            _service = service;
        }

        // GET: api/Farm
        [HttpGet]
        [Authorize(Policy = Permissions.FarmRead)]
        public async Task<IActionResult> GetAll()
        {
            var farms = await _service.GetAllAsync();

            var result = farms.Select(MapFarm).ToList();

            return Ok(result);
        }

        // GET: api/Farm/1
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.FarmRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var farm = await _service.GetByIdAsync(id);

            if (farm == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy trang trại"
                });
            }

            return Ok(MapFarm(farm));
        }

        // POST: api/Farm
        // Admin: chọn OwnerId tùy ý. Farmer: luôn là chính mình.
        [HttpPost]
        [Authorize(Policy = Permissions.FarmCreate)]
        public async Task<IActionResult> Create([FromBody] FarmCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var farm = new Farm
            {
                Name = dto.Name,
                Location = dto.Location,
                Area = dto.Area,
                OwnerId = User.IsInRole("Admin") ? dto.OwnerId : userId
            };

            var createdFarm = await _service.CreateAsync(farm);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdFarm.Id },
                MapFarm(createdFarm));
        }

        // PUT: api/Farm/1
        // Admin: sửa tất cả. Farmer: chỉ sửa Farm của mình.
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.FarmUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] FarmCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");

            var existing = await _service.GetByIdAsync(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy trang trại"
                });
            }

            if (!isAdmin && existing.OwnerId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Bạn không có quyền sửa trang trại này"
                });
            }

            var farm = new Farm
            {
                Name = dto.Name,
                Location = dto.Location,
                Area = dto.Area,
                OwnerId = isAdmin ? dto.OwnerId : existing.OwnerId
            };

            var updatedFarm = await _service.UpdateAsync(id, farm);

            if (updatedFarm == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy trang trại"
                });
            }

            return Ok(MapFarm(updatedFarm));
        }

        // DELETE: api/Farm/1
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.FarmDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy trang trại"
                });
            }

            return Ok(new
            {
                message = "Xóa trang trại thành công"
            });
        }

        // Lấy Id người dùng từ claim NameIdentifier trong JWT
        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out userId);
        }

        // Chuyển Entity Farm thành DTO an toàn
        private static FarmResponseDto MapFarm(Farm farm)
        {
            return new FarmResponseDto
            {
                Id = farm.Id,
                Name = farm.Name,
                Location = farm.Location,
                Area = farm.Area,
                OwnerId = farm.OwnerId,
                CreatedAt = farm.CreatedAt,

                Owner = farm.Owner == null ? null : new FarmOwnerDto
                {
                    Id = farm.Owner.Id,
                    Username = farm.Owner.Username,
                    Email = farm.Owner.Email,
                    FullName = farm.Owner.FullName,
                    Role = farm.Owner.Role
                },

                Fields = farm.Fields.Select(field => new FarmFieldDto
                {
                    Id = field.Id,
                    Name = field.Name,
                    FarmId = field.FarmId,
                    SoilType = field.SoilType,
                    Area = field.Area,
                    Description = field.Description
                }).ToList()
            };
        }
    }
}