using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FieldController : ControllerBase
    {
        private readonly IFieldService _service;

        public FieldController(IFieldService service)
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
            var field = await _service.GetByIdAsync(id);

            if (field == null)
                return NotFound(new { message = "Không tìm thấy thửa ruộng" });

            return Ok(field);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Field field)
        {
            var created = await _service.CreateAsync(field);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Field field)
        {
            var updated = await _service.UpdateAsync(id, field);

            if (updated == null)
                return NotFound(new { message = "Không tìm thấy thửa ruộng" });

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new { message = "Không tìm thấy thửa ruộng" });

            return Ok(new { message = "Xóa thửa ruộng thành công" });
        }
    }
}