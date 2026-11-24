using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication1.Authorization;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.DTOs.Plantings;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantingController : ControllerBase
    {
        private readonly IPlantingService _service;
        private readonly IOwnershipService _ownership;

        public PlantingController(
            IPlantingService service,
            IOwnershipService ownership)
        {
            _service = service;
            _ownership = ownership;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.PlantingRead)]
        public async Task<IActionResult> GetAll()
        {
            var plantings = await _service.GetAllAsync();

            return Ok(plantings.Select(MapPlanting).ToList());
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.PlantingRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var planting = await _service.GetByIdAsync(id);

            if (planting == null)
                return NotFound(new
                {
                    message = "Không tìm thấy vụ trồng"
                });

            return Ok(MapPlanting(planting));
        }

        // Farmer chỉ tạo Planting trong Field của mình
        [HttpPost]
        [Authorize(Policy = Permissions.PlantingCreate)]
        public async Task<IActionResult> Create(
            [FromBody] PlantingCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (!User.IsInRole("Admin") &&
                !await _ownership.OwnsFieldAsync(dto.FieldId, userId))
            {
                return Forbidden("Bạn không có quyền thao tác trên thửa ruộng này");
            }

            var planting = new Planting
            {
                FieldId = dto.FieldId,
                CropId = dto.CropId,
                PlantingDate = dto.PlantingDate,
                ExpectedHarvestDate = dto.ExpectedHarvestDate,
                Status = dto.Status
            };

            var created = await _service.CreateAsync(planting);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                MapPlanting(created)
            );
        }

        // Farmer chỉ sửa Planting của mình và không chuyển sang Field người khác
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.PlantingUpdate)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] PlantingCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (!User.IsInRole("Admin"))
            {
                if (!await _ownership.OwnsPlantingAsync(id, userId))
                    return Forbidden("Bạn không có quyền sửa vụ trồng này");

                if (!await _ownership.OwnsFieldAsync(dto.FieldId, userId))
                    return Forbidden("Bạn không có quyền thao tác trên thửa ruộng này");
            }

            var planting = new Planting
            {
                FieldId = dto.FieldId,
                CropId = dto.CropId,
                PlantingDate = dto.PlantingDate,
                ExpectedHarvestDate = dto.ExpectedHarvestDate,
                Status = dto.Status
            };

            var updated = await _service.UpdateAsync(id, planting);

            if (updated == null)
                return NotFound(new
                {
                    message = "Không tìm thấy vụ trồng"
                });

            return Ok(MapPlanting(updated));
        }

        // Farmer chỉ xóa Planting của mình
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.PlantingDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (!User.IsInRole("Admin") &&
                !await _ownership.OwnsPlantingAsync(id, userId))
            {
                return Forbidden("Bạn không có quyền xóa vụ trồng này");
            }

            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new
                {
                    message = "Không tìm thấy vụ trồng"
                });

            return Ok(new
            {
                message = "Xóa vụ trồng thành công"
            });
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out userId);
        }

        private ObjectResult Forbidden(string message)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message });
        }

        private static PlantingResponseDto MapPlanting(Planting planting)
        {
            return new PlantingResponseDto
            {
                Id = planting.Id,
                FieldId = planting.FieldId,
                CropId = planting.CropId,
                PlantingDate = planting.PlantingDate,
                ExpectedHarvestDate = planting.ExpectedHarvestDate,
                Status = planting.Status
            };
        }
    }
}