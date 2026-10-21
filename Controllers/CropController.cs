using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CropController : ControllerBase
    {
        private readonly ICropService _service;

        public CropController(ICropService service)
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
            var crop = await _service.GetByIdAsync(id);

            if (crop == null)
                return NotFound(new
                {
                    message = "Không tìm thấy cây trồng"
                });

            return Ok(crop);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Crop crop)
        {
            var created = await _service.CreateAsync(crop);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Crop crop)
        {
            var updated = await _service.UpdateAsync(id, crop);

            if (updated == null)
                return NotFound(new
                {
                    message = "Không tìm thấy cây trồng"
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
                    message = "Không tìm thấy cây trồng"
                });

            return Ok(new
            {
                message = "Xóa cây trồng thành công"
            });
        }
    }
}