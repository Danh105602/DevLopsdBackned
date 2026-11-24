using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication1.Authorization;
using WebApplication1.DTOs.Fields;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FieldController : ControllerBase
    {
        private readonly IFieldService _service;
        private readonly IOwnershipService _ownership;

        public FieldController(
            IFieldService service,
            IOwnershipService ownership)
        {
            _service = service;
            _ownership = ownership;
        }

        // GET: api/Field
        // Permission: Field.Read
        [HttpGet]
        [Authorize(Policy = Permissions.FieldRead)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET: api/Field/1
        // Permission: Field.Read
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.FieldRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var field = await _service.GetByIdAsync(id);

            if (field == null)
                return NotFound(new
                {
                    message = "Không tìm thấy thửa ruộng"
                });

            return Ok(field);
        }

        // POST: api/Field
        // Farmer chỉ tạo Field trong Farm của mình
        [HttpPost]
        [Authorize(Policy = Permissions.FieldCreate)]
        public async Task<IActionResult> Create([FromBody] FieldCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (!User.IsInRole("Admin") &&
                !await _ownership.OwnsFarmAsync(dto.FarmId, userId))
            {
                return Forbidden("Bạn không có quyền thao tác trên trang trại này");
            }

            var field = new Field
            {
                Name = dto.Name,
                FarmId = dto.FarmId,
                SoilType = dto.SoilType,
                Area = dto.Area,
                Description = dto.Description
            };

            var created = await _service.CreateAsync(field);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        // PUT: api/Field/1
        // Farmer chỉ sửa Field của mình và không chuyển sang Farm người khác
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.FieldUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] FieldCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (!User.IsInRole("Admin"))
            {
                if (!await _ownership.OwnsFieldAsync(id, userId))
                    return Forbidden("Bạn không có quyền sửa thửa ruộng này");

                if (!await _ownership.OwnsFarmAsync(dto.FarmId, userId))
                    return Forbidden("Bạn không có quyền thao tác trên trang trại này");
            }

            var field = new Field
            {
                Name = dto.Name,
                FarmId = dto.FarmId,
                SoilType = dto.SoilType,
                Area = dto.Area,
                Description = dto.Description
            };

            var updated = await _service.UpdateAsync(id, field);

            if (updated == null)
                return NotFound(new
                {
                    message = "Không tìm thấy thửa ruộng"
                });

            return Ok(updated);
        }

        // DELETE: api/Field/1
        // Farmer chỉ xóa Field của mình
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.FieldDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (!User.IsInRole("Admin") &&
                !await _ownership.OwnsFieldAsync(id, userId))
            {
                return Forbidden("Bạn không có quyền xóa thửa ruộng này");
            }

            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new
                {
                    message = "Không tìm thấy thửa ruộng"
                });

            return Ok(new
            {
                message = "Xóa thửa ruộng thành công"
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
    }
}