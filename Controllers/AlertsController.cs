using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Authorization;
using WebApplication1.DTOs.Sensors;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize(Policy = Permissions.AlertRead)]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alerts;
    private readonly IOwnershipService _ownership;

    public AlertsController(IAlertService alerts, IOwnershipService ownership)
    {
        _alerts = alerts;
        _ownership = ownership;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? farmId, [FromQuery] string? status)
    {
        int? ownerId = null;
        if (!User.IsInRole("Admin"))
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            if (farmId.HasValue && !await _ownership.OwnsFarmAsync(farmId.Value, userId)) return Forbid();
            ownerId = userId;
        }
        if (!string.IsNullOrWhiteSpace(status) && status is not (AlertStatuses.New or AlertStatuses.Seen or AlertStatuses.Resolved))
            return BadRequest(new { message = "Status is invalid." });
        var alerts = await _alerts.GetAlertsAsync(ownerId, farmId, status);
        return Ok(alerts.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var alert = await _alerts.GetAlertByIdAsync(id);
        if (alert == null) return NotFound();
        if (alert.SensorDevice?.Field == null || !await CanAccessField(alert.SensorDevice.FieldId)) return Forbid();
        return Ok(Map(alert));
    }

    [HttpPost("{id:int}/seen")]
    [Authorize(Policy = Permissions.AlertUpdate)]
    public async Task<IActionResult> MarkSeen(int id)
    {
        var current = await _alerts.GetAlertByIdAsync(id);
        if (current == null) return NotFound();
        if (current.SensorDevice?.Field == null || !await CanAccessField(current.SensorDevice.FieldId)) return Forbid();
        var updated = await _alerts.MarkSeenAsync(id);
        if (updated == null) return NotFound();
        return Ok(Map((await _alerts.GetAlertByIdAsync(id))!));
    }

    [HttpPost("{id:int}/resolve")]
    [Authorize(Policy = Permissions.AlertUpdate)]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Resolve(int id)
    {
        var resolved = await _alerts.ResolveAsync(id);
        if (resolved == null) return NotFound();
        return Ok(Map((await _alerts.GetAlertByIdAsync(id))!));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.AlertDelete)]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var resolved = await _alerts.ResolveAsync(id);
        return resolved == null ? NotFound() : Ok(new { message = "Alert resolved; history was retained." });
    }

    private async Task<bool> CanAccessField(int fieldId)
    {
        if (User.IsInRole("Admin")) return true;
        return TryGetUserId(out var userId) && await _ownership.OwnsFieldAsync(fieldId, userId);
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private static AlertDto Map(Alert alert) => new()
    {
        Id = alert.Id,
        AlertRuleId = alert.AlertRuleId,
        SensorDeviceId = alert.SensorDeviceId,
        SensorReadingId = alert.SensorReadingId,
        Severity = alert.Severity,
        Message = alert.Message,
        Status = alert.Status,
        TriggeredAtUtc = alert.TriggeredAtUtc,
        SeenAtUtc = alert.SeenAtUtc,
        ResolvedAtUtc = alert.ResolvedAtUtc,
        FieldId = alert.SensorDevice?.FieldId ?? 0
    };
}
