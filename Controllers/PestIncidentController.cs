using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Authorization;
using WebApplication1.DTOs.PestIncident;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PestIncidentController : ControllerBase
    {
        private readonly IPestIncidentService _service;
        private readonly IOwnershipService _ownership;
        private readonly IAuditLogService _auditLog;

        public PestIncidentController(IPestIncidentService service, IOwnershipService ownership, IAuditLogService auditLog)
        {
            _service = service;
            _ownership = ownership;
            _auditLog = auditLog;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.PestIncidentRead)]
        public async Task<IActionResult> GetAll()
        {
            var incidents = await _service.GetAllAsync();
            if (!User.IsInRole("Admin"))
            {
                if (!TryGetUserId(out var userId)) return Unauthorized();
                var owned = new List<PestIncident>();
                foreach (var incident in incidents)
                    if (await _ownership.OwnsPlantingAsync(incident.PlantingId, userId)) owned.Add(incident);
                incidents = owned;
            }
            await _auditLog.RecordReadAsync("PestIncident", 0, isList: true);
            return Ok(incidents.Select(Map).ToList());
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = Permissions.PestIncidentRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var incident = await _service.GetByIdAsync(id);
            if (incident == null) return NotFound(new { message = "Không tìm thấy sự cố sâu bệnh" });
            if (!await CanAccessPlanting(incident.PlantingId)) return Forbidden();
            await _auditLog.RecordReadAsync("PestIncident", id);
            return Ok(Map(incident));
        }

        [HttpPost]
        [Authorize(Policy = Permissions.PestIncidentCreate)]
        public async Task<IActionResult> Create([FromBody] PestIncidentCreateDto dto)
        {
            if (!User.IsInRole("Admin") && !await CanAccessPlanting(dto.PlantingId))
                return Forbidden();
            if (!await _ownership.PlantingExistsAsync(dto.PlantingId))
                return NotFound(new { message = "Không tìm thấy vụ trồng" });
            if (!PestIncidentSeverities.IsValid(dto.Severity))
                return BadRequest(new { message = "Mức độ không hợp lệ" });

            var dateError = await ValidateDetectedDate(dto.PlantingId, dto.DetectedDate);
            if (dateError != null) return BadRequest(new { message = dateError });

            var incident = new PestIncident
            {
                PlantingId = dto.PlantingId,
                PestName = dto.PestName.Trim(),
                Severity = dto.Severity,
                DetectedDate = dto.DetectedDate,
                Treatment = dto.Treatment,
                Status = PestIncidentStatuses.Open,
                Notes = dto.Notes
            };
            var created = await _service.CreateAsync(incident);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Permissions.PestIncidentUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] PestIncidentUpdateDto dto)
        {
            var current = await _service.GetByIdAsync(id);
            if (current == null) return NotFound(new { message = "Không tìm thấy sự cố sâu bệnh" });
            if (!await CanAccessPlanting(current.PlantingId)) return Forbidden();
            if (!PestIncidentSeverities.IsValid(dto.Severity))
                return BadRequest(new { message = "Mức độ không hợp lệ" });
            if (!PestIncidentStatuses.IsValid(dto.Status))
                return BadRequest(new { message = "Trạng thái không hợp lệ" });
            if (!CanTransition(current.Status, dto.Status))
                return Conflict(new { message = "Trạng thái chỉ được chuyển từ Open sang Treated rồi Resolved" });

            if (dto.Status == PestIncidentStatuses.Resolved)
            {
                if (dto.ResolutionDate == null)
                    return BadRequest(new { message = "Cần ngày giải quyết khi trạng thái là Resolved" });
                if (dto.ResolutionDate.Value.Date < dto.DetectedDate.Date)
                    return BadRequest(new { message = "Ngày giải quyết không được trước ngày phát hiện" });
            }
            else if (dto.ResolutionDate != null)
            {
                return BadRequest(new { message = "Chỉ sự cố Resolved mới có ngày giải quyết" });
            }

            var dateError = await ValidateDetectedDate(current.PlantingId, dto.DetectedDate);
            if (dateError != null) return BadRequest(new { message = dateError });

            var updated = await _service.UpdateAsync(id, new PestIncident
            {
                PestName = dto.PestName.Trim(),
                Severity = dto.Severity,
                DetectedDate = dto.DetectedDate,
                Treatment = dto.Treatment,
                Status = dto.Status,
                ResolutionDate = dto.ResolutionDate,
                Notes = dto.Notes
            });
            return updated == null ? NotFound() : Ok(Map(updated));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Permissions.PestIncidentDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(new { message = "Không tìm thấy sự cố sâu bệnh" });
            return Ok(new { message = "Đã xóa sự cố sâu bệnh" });
        }

        private async Task<string?> ValidateDetectedDate(int plantingId, DateTime detectedDate)
        {
            if (detectedDate.Date > DateTime.UtcNow.Date)
                return "Ngày phát hiện không được ở tương lai";

            var plantingDate = await _ownership.GetPlantingDateAsync(plantingId);
            if (plantingDate == null)
                return "Không tìm thấy vụ trồng";
            if (detectedDate.Date < plantingDate.Value.Date)
                return "Ngày phát hiện không được trước ngày gieo trồng";
            return null;
        }

        private static bool CanTransition(string current, string next) =>
            current == next ||
            (current == PestIncidentStatuses.Open && next == PestIncidentStatuses.Treated) ||
            (current == PestIncidentStatuses.Treated && next == PestIncidentStatuses.Resolved);

        private async Task<bool> CanAccessPlanting(int plantingId)
        {
            if (User.IsInRole("Admin")) return true;
            return TryGetUserId(out var userId) && await _ownership.OwnsPlantingAsync(plantingId, userId);
        }

        private bool TryGetUserId(out int userId) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

        private ObjectResult Forbidden() => StatusCode(StatusCodes.Status403Forbidden,
            new { message = "Bạn không có quyền thao tác trên sự cố sâu bệnh này" });

        private static PestIncidentResponseDto Map(PestIncident item) => new()
        {
            Id = item.Id,
            PlantingId = item.PlantingId,
            PestName = item.PestName,
            Severity = item.Severity,
            DetectedDate = item.DetectedDate,
            Treatment = item.Treatment,
            Status = item.Status,
            ResolutionDate = item.ResolutionDate,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt
        };
    }
}
