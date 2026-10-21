using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlantingController : ControllerBase
    {
        private readonly IPlantingService _service;

        public PlantingController(IPlantingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var planting = await _service.GetByIdAsync(id);

            if (planting == null)
                return NotFound(new
                {
                    message = "Không tìm thấy vụ trồng"
                });

            return Ok(planting);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Planting planting)
        {
            var created = await _service.CreateAsync(planting);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            Planting planting)
        {
            var updated = await _service.UpdateAsync(id, planting);

            if (updated == null)
                return NotFound(new
                {
                    message = "Không tìm thấy vụ trồng"
                });

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
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
    }
}