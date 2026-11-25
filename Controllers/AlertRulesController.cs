using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Authorization;
using WebApplication1.DTOs.Sensors;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/alert-rules")]
[Authorize(Policy = Permissions.AlertRuleRead)]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertService _alerts;
    private readonly IOwnershipService _ownership;

    public AlertRulesController(IAlertService alerts, IOwnershipService ownership)
    {
        _alerts = alerts;
        _ownership = ownership;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? fieldId)
    {
        int? ownerId = null;
        if (!User.IsInRole("Admin"))
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            if (fieldId.HasValue && !await _ownership.OwnsFieldAsync(fieldId.Value, userId)) return Forbid();
            ownerId = userId;
        }
        var rules = await _alerts.GetRulesAsync(ownerId, fieldId);
        return Ok(rules.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rule = await _alerts.GetRuleByIdAsync(id);
        if (rule == null) return NotFound();
        if (!await CanAccessField(rule.FieldId)) return Forbid();
        return Ok(Map(rule));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.AlertRuleCreate)]
    public async Task<IActionResult> Create([FromBody] AlertRuleCreateDto dto)
    {
        if (!await CanAccessField(dto.FieldId)) return Forbid();
        if (!await _ownership.FieldExistsAsync(dto.FieldId)) return NotFound();
        var validation = Validate(dto.Metric, dto.LowerThreshold, dto.UpperThreshold, dto.Severity);
        if (validation != null) return BadRequest(new { message = validation });
        if ((await _alerts.GetRulesAsync(null, dto.FieldId)).Any(r => r.Metric == dto.Metric))
            return Conflict(new { message = "A rule already exists for this Field and metric." });

        var rule = new AlertRule
        {
            FieldId = dto.FieldId,
            Metric = dto.Metric,
            LowerThreshold = dto.LowerThreshold,
            UpperThreshold = dto.UpperThreshold,
            ConsecutiveReadings = dto.ConsecutiveReadings,
            CooldownMinutes = dto.CooldownMinutes,
            Severity = dto.Severity,
            IsEnabled = true
        };
        var created = await _alerts.CreateRuleAsync(rule);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.AlertRuleUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] AlertRuleUpdateDto dto)
    {
        var existing = await _alerts.GetRuleByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await CanAccessField(existing.FieldId)) return Forbid();
        var validation = Validate(existing.Metric, dto.LowerThreshold, dto.UpperThreshold, dto.Severity);
        if (validation != null) return BadRequest(new { message = validation });

        var updated = await _alerts.UpdateRuleAsync(id, new AlertRule
        {
            LowerThreshold = dto.LowerThreshold,
            UpperThreshold = dto.UpperThreshold,
            ConsecutiveReadings = dto.ConsecutiveReadings,
            CooldownMinutes = dto.CooldownMinutes,
            Severity = dto.Severity,
            IsEnabled = dto.IsEnabled
        });
        return updated == null ? NotFound() : Ok(Map(updated));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.AlertRuleDelete)]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var disabled = await _alerts.DisableRuleAsync(id);
        return disabled ? Ok(new { message = "Alert rule disabled; history was retained." }) : NotFound();
    }

    private async Task<bool> CanAccessField(int fieldId)
    {
        if (User.IsInRole("Admin")) return true;
        return TryGetUserId(out var userId) && await _ownership.OwnsFieldAsync(fieldId, userId);
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private static string? Validate(string metric, decimal? lower, decimal? upper, string severity)
    {
        if (!SensorMetrics.IsValid(metric)) return "Metric is invalid.";
        if (metric == SensorMetrics.DeviceOffline)
        {
            if (lower.HasValue || !upper.HasValue || upper < 1 || upper > 10080)
                return "DeviceOffline requires an upper threshold from 1 to 10080 minutes.";
            return severity is AlertSeverities.Normal or AlertSeverities.Urgent ? null : "Severity is invalid.";
        }
        if (lower == null && upper == null) return "At least one threshold is required.";
        if (lower.HasValue && upper.HasValue && lower.Value >= upper.Value) return "Lower threshold must be below upper threshold.";
        if (severity is not (AlertSeverities.Normal or AlertSeverities.Urgent)) return "Severity is invalid.";
        var range = metric switch
        {
            SensorMetrics.Temperature => (-50m, 80m),
            SensorMetrics.AirHumidity or SensorMetrics.SoilMoisture => (0m, 100m),
            SensorMetrics.PH => (0m, 14m),
            _ => (decimal.MinValue, decimal.MaxValue)
        };
        if ((lower.HasValue && (lower < range.Item1 || lower > range.Item2)) ||
            (upper.HasValue && (upper < range.Item1 || upper > range.Item2)))
            return "Threshold is outside the physical range for this metric.";
        return null;
    }

    private static AlertRuleDto Map(AlertRule rule) => new()
    {
        Id = rule.Id,
        FieldId = rule.FieldId,
        Metric = rule.Metric,
        LowerThreshold = rule.LowerThreshold,
        UpperThreshold = rule.UpperThreshold,
        ConsecutiveReadings = rule.ConsecutiveReadings,
        CooldownMinutes = rule.CooldownMinutes,
        Severity = rule.Severity,
        IsEnabled = rule.IsEnabled
    };
}
