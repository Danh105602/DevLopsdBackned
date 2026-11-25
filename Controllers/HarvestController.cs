
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication1.Authorization;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.DTOs.Harvest;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HarvestController : ControllerBase
    {
        private readonly IHarvestService _service;
        private readonly IOwnershipService _ownership;

        public HarvestController(
            IHarvestService service,
            IOwnershipService ownership)
        {
            _service = service;
            _ownership = ownership;
        }

        // GET: api/Harvest
        [HttpGet]
        [Authorize(Policy = Permissions.HarvestRead)]
        public async Task<IActionResult> GetAll()
        {
            var harvests = await _service.GetAllAsync();

            // Farmer chỉ xem thu hoạch thuộc Planting của mình
            if (!User.IsInRole("Admin"))
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var ownedHarvests = new List<Harvest>();

                foreach (var harvest in harvests)
                {
                    if (await _ownership.OwnsPlantingAsync(
                        harvest.PlantingId, userId))
                    {
                        ownedHarvests.Add(harvest);
                    }
                }

                harvests = ownedHarvests;
            }

            return Ok(harvests.Select(MapHarvest).ToList());
        }

        // GET: api/Harvest/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.HarvestRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var harvest = await _service.GetByIdAsync(id);

            if (harvest == null)
                return NotFound(new
                {
                    message = "Không tìm thấy bản ghi thu hoạch"
                });

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                if (!await _ownership.OwnsPlantingAsync(
                    harvest.PlantingId, userId))
                {
                    return Forbidden(
                        "Bạn không có quyền xem bản ghi thu hoạch này");
                }
            }

            return Ok(MapHarvest(harvest));
        }

        // POST: api/Harvest
        [HttpPost]
        [Authorize(Policy = Permissions.HarvestCreate)]
        public async Task<IActionResult> Create(
            [FromBody] HarvestCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (!User.IsInRole("Admin") &&
                !await _ownership.OwnsPlantingAsync(
                    dto.PlantingId, userId))
            {
                return Forbidden(
                    "Bạn không có quyền tạo thu hoạch cho vụ trồng này");
            }

            if (!await _ownership.PlantingExistsAsync(dto.PlantingId))
                return NotFound();

            var createDateError = await ValidateHarvestDate(dto.PlantingId, dto.HarvestDate);
            if (createDateError != null)
                return BadRequest(new { message = createDateError });

            var harvest = new Harvest
            {
                PlantingId = dto.PlantingId,
                HarvestDate = dto.HarvestDate,
                QuantityKg = dto.QuantityKg,
                Quality = dto.Quality,
                Notes = dto.Notes
            };

            var created = await _service.CreateAsync(harvest);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                MapHarvest(created)
            );
        }

        // PUT: api/Harvest/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.HarvestUpdate)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] HarvestUpdateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var existing = await _service.GetByIdAsync(id);

            if (existing == null)
                return NotFound(new
                {
                    message = "Không tìm thấy bản ghi thu hoạch"
                });

            if (!User.IsInRole("Admin") &&
                !await _ownership.OwnsPlantingAsync(
                    existing.PlantingId, userId))
            {
                return Forbidden(
                    "Bạn không có quyền sửa bản ghi thu hoạch này");
            }

            var updateDateError = await ValidateHarvestDate(existing.PlantingId, dto.HarvestDate);
            if (updateDateError != null)
                return BadRequest(new { message = updateDateError });

            var harvest = new Harvest
            {
                HarvestDate = dto.HarvestDate,
                QuantityKg = dto.QuantityKg,
                Quality = dto.Quality,
                Notes = dto.Notes
            };

            var updated = await _service.UpdateAsync(id, harvest);

            if (updated == null)
                return NotFound();

            return Ok(MapHarvest(updated));
        }

        // DELETE: api/Harvest/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var existing = await _service.GetByIdAsync(id);

            if (existing == null)
                return NotFound(new
                {
                    message = "Không tìm thấy bản ghi thu hoạch"
                });

            if (!User.IsInRole("Admin") &&
                !await _ownership.OwnsPlantingAsync(
                    existing.PlantingId, userId))
            {
                return Forbidden(
                    "Bạn không có quyền xóa bản ghi thu hoạch này");
            }

            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound();

            return Ok(new
            {
                message = "Xóa bản ghi thu hoạch thành công"
            });
        }

        private async Task<string?> ValidateHarvestDate(int plantingId, DateTime harvestDate)
        {
            if (harvestDate.Date > DateTime.UtcNow.Date)
                return "Harvest date cannot be in the future.";

            var plantingDate = await _ownership.GetPlantingDateAsync(plantingId);
            if (plantingDate == null)
                return "Planting was not found.";
            if (harvestDate.Date < plantingDate.Value.Date)
                return "Harvest date cannot be before the planting date.";

            return null;
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out userId);
        }

        private ObjectResult Forbidden(string message)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message });
        }

        private static HarvestResponseDto MapHarvest(Harvest harvest)
        {
            return new HarvestResponseDto
            {
                Id = harvest.Id,
                PlantingId = harvest.PlantingId,
                HarvestDate = harvest.HarvestDate,
                QuantityKg = harvest.QuantityKg,
                Quality = harvest.Quality,
                Notes = harvest.Notes,
                CreatedAt = harvest.CreatedAt
            };
        }
    }
}
