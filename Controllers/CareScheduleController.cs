using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Authorization;
using WebApplication1.DTOs.CareSchedule;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CareScheduleController : ControllerBase
    {
        private readonly ICareScheduleService _service;
        private readonly IOwnershipService _ownership;

        public CareScheduleController(ICareScheduleService service, IOwnershipService ownership)
        {
            _service = service;
            _ownership = ownership;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.CareScheduleRead)]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _service.GetAllAsync();
            if (!User.IsInRole("Admin"))
            {
                if (!TryGetUserId(out var userId)) return Unauthorized();
                var owned = new List<CareSchedule>();
                foreach (var item in schedules)
                    if (await _ownership.OwnsPlantingAsync(item.PlantingId, userId)) owned.Add(item);
                schedules = owned;
            }
            return Ok(schedules.Select(Map).ToList());
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = Permissions.CareScheduleRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var schedule = await _service.GetByIdAsync(id);
            if (schedule == null) return NotFound(new { message = "Không tìm thấy lịch chăm sóc" });
            if (!await CanAccessPlanting(schedule.PlantingId)) return Forbidden();
            return Ok(Map(schedule));
        }

        [HttpPost]
        [Authorize(Policy = Permissions.CareScheduleCreate)]
        public async Task<IActionResult> Create([FromBody] CareScheduleCreateDto dto)
        {
            if (!await _ownership.PlantingExistsAsync(dto.PlantingId))
                return NotFound(new { message = "Không tìm thấy vụ trồng" });
            if (!await CanAccessPlanting(dto.PlantingId)) return Forbidden();
            if (!CareScheduleTaskTypes.IsValid(dto.TaskType))
                return BadRequest(new { message = "Loại việc không hợp lệ" });
            var schedule = new CareSchedule
            {
                PlantingId = dto.PlantingId,
                TaskType = dto.TaskType.Trim(),
                ScheduledDate = dto.ScheduledDate,
                Status = CareScheduleStatuses.Pending,
                Notes = dto.Notes
            };
            var created = await _service.CreateAsync(schedule);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Permissions.CareScheduleUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] CareScheduleUpdateDto dto)
        {
            var current = await _service.GetByIdAsync(id);
            if (current == null) return NotFound(new { message = "Không tìm thấy lịch chăm sóc" });
            if (!await CanAccessPlanting(current.PlantingId)) return Forbidden();
            if (!CareScheduleTaskTypes.IsValid(dto.TaskType))
                return BadRequest(new { message = "Loại việc không hợp lệ" });
            if (!CareScheduleStatuses.IsValid(dto.Status))
                return BadRequest(new { message = "Trạng thái không hợp lệ" });
            if ((current.Status == CareScheduleStatuses.Completed && dto.Status != CareScheduleStatuses.Completed) ||
                (current.Status == CareScheduleStatuses.Skipped && dto.Status == CareScheduleStatuses.Completed) ||
                (dto.Status == CareScheduleStatuses.Completed && current.Status != CareScheduleStatuses.Completed))
                return Conflict(new { message = "Không thể chuyển trạng thái trực tiếp; dùng thao tác đánh dấu hoàn thành" });

            var updated = await _service.UpdateAsync(id, new CareSchedule
            {
                TaskType = dto.TaskType.Trim(),
                ScheduledDate = dto.ScheduledDate,
                Status = dto.Status,
                Notes = dto.Notes
            });
            return updated == null ? NotFound() : Ok(Map(updated));
        }

        [HttpPost("{id:int}/complete")]
        [Authorize(Policy = Permissions.CareScheduleUpdate)]
        public async Task<IActionResult> Complete(int id)
        {
            var current = await _service.GetByIdAsync(id);
            if (current == null) return NotFound(new { message = "Không tìm thấy lịch chăm sóc" });
            if (!await CanAccessPlanting(current.PlantingId)) return Forbidden();
            if (current.Status == CareScheduleStatuses.Skipped)
                return Conflict(new { message = "Lịch đã bỏ qua không thể đánh dấu hoàn thành" });

            await _service.CompleteAsync(id);
            var completed = await _service.GetByIdAsync(id);
            return Ok(Map(completed!));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Permissions.CareScheduleDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var current = await _service.GetByIdAsync(id);
            if (current == null) return NotFound(new { message = "Không tìm thấy lịch chăm sóc" });
            if (!await CanAccessPlanting(current.PlantingId)) return Forbidden();
            await _service.DeleteAsync(id);
            return Ok(new { message = "Đã xóa lịch chăm sóc" });
        }

        private async Task<bool> CanAccessPlanting(int plantingId)
        {
            if (User.IsInRole("Admin")) return true;
            return TryGetUserId(out var userId) && await _ownership.OwnsPlantingAsync(plantingId, userId);
        }

        private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

        private ObjectResult Forbidden() => StatusCode(StatusCodes.Status403Forbidden,
            new { message = "Bạn không có quyền thao tác trên lịch chăm sóc này" });

        private static CareScheduleResponseDto Map(CareSchedule item) => new()
        {
            Id = item.Id,
            PlantingId = item.PlantingId,
            TaskType = item.TaskType,
            ScheduledDate = item.ScheduledDate,
            Status = item.Status,
            CompletedAt = item.CompletedAt,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt,
            IsOverdue = item.Status == CareScheduleStatuses.Pending && item.ScheduledDate.Date < DateTime.UtcNow.Date
        };
    }
}
