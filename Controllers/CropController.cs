
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication1.Authorization;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.DTOs.Crop;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CropController : ControllerBase
    {
        private readonly ICropService _service;

        public CropController(ICropService service)
        {
            _service = service;
        }

        // GET: api/Crop
        [HttpGet]
        [Authorize(Policy = Permissions.CropRead)]
        public async Task<IActionResult> GetAll()
        {
            var crops = await _service.GetAllAsync();

            var result = crops.Select(MapCrop).ToList();

            return Ok(result);
        }

        // GET: api/Crop/1
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.CropRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var crop = await _service.GetByIdAsync(id);

            if (crop == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy cây trồng"
                });
            }

            return Ok(MapCrop(crop));
        }

        // POST: api/Crop
        [HttpPost]
        [Authorize(Policy = Permissions.CropCreate)]
        public async Task<IActionResult> Create(
            [FromBody] CropCreateDto dto)
        {
            var crop = new Crop
            {
                Name = dto.Name,
                Category = dto.Category,
                Description = dto.Description,
                GrowthDays = dto.GrowthDays
            };

            var created = await _service.CreateAsync(crop);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                MapCrop(created));
        }

        // PUT: api/Crop/1
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.CropUpdate)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CropCreateDto dto)
        {
            var crop = new Crop
            {
                Name = dto.Name,
                Category = dto.Category,
                Description = dto.Description,
                GrowthDays = dto.GrowthDays
            };

            var updated = await _service.UpdateAsync(id, crop);

            if (updated == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy cây trồng"
                });
            }

            return Ok(MapCrop(updated));
        }

        // DELETE: api/Crop/1
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.CropDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy cây trồng"
                });
            }

            return Ok(new
            {
                message = "Xóa cây trồng thành công"
            });
        }

        private static CropResponseDto MapCrop(Crop crop)
        {
            return new CropResponseDto
            {
                Id = crop.Id,
                Name = crop.Name,
                Category = crop.Category,
                Description = crop.Description,
                GrowthDays = crop.GrowthDays
            };
        }
    }
}
