using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Authorization;
using WebApplication1.DTOs.Sensors;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/sensor-devices/{deviceId:int}/readings")]
[Authorize(Policy = Permissions.SensorReadingRead)]
public class SensorReadingsController : ControllerBase
{
    private readonly ISensorDeviceService _devices;
    private readonly ISensorTelemetryService _telemetry;
    private readonly IOwnershipService _ownership;

    public SensorReadingsController(
        ISensorDeviceService devices,
        ISensorTelemetryService telemetry,
        IOwnershipService ownership)
    {
        _devices = devices;
        _telemetry = telemetry;
        _ownership = ownership;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(
        int deviceId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string bucket = "hour")
    {
        if (!string.Equals(bucket, "hour", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(bucket, "day", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Bucket must be hour or day." });

        var device = await _devices.GetByIdAsync(deviceId);
        if (device == null) return NotFound();
        if (!await CanAccessField(device.FieldId)) return Forbid();

        var endUtc = to?.UtcDateTime ?? DateTime.UtcNow;
        var startUtc = from?.UtcDateTime ?? endUtc.AddDays(-30);
        if (endUtc <= startUtc || endUtc > DateTime.UtcNow.AddMinutes(5) || endUtc - startUtc > TimeSpan.FromDays(365))
            return BadRequest(new { message = "Time range must be positive, no more than one year, and not in the future." });

        return Ok(await _telemetry.GetHistoryAsync(deviceId, startUtc, endUtc, bucket));
    }

    private async Task<bool> CanAccessField(int fieldId)
    {
        if (User.IsInRole("Admin")) return true;
        return TryGetUserId(out var userId) && await _ownership.OwnsFieldAsync(fieldId, userId);
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}

[ApiController]
[Route("api/iot/readings")]
[AllowAnonymous]
public class SensorIngestController : ControllerBase
{
    private readonly ISensorDeviceService _devices;
    private readonly ISensorTelemetryService _telemetry;
    private readonly SensorDeviceRateLimiter _rateLimiter;

    public SensorIngestController(
        ISensorDeviceService devices,
        ISensorTelemetryService telemetry,
        SensorDeviceRateLimiter rateLimiter)
    {
        _devices = devices;
        _telemetry = telemetry;
        _rateLimiter = rateLimiter;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromHeader(Name = "X-Device-Key")] string apiKey,
        [FromBody] SensorReadingIngestDto dto)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return Unauthorized();
        var device = await _devices.AuthenticateAsync(dto.DeviceCode, apiKey);
        if (device == null) return Unauthorized();
        if (!_rateLimiter.TryAcquire(device.Id))
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Device rate limit exceeded." });

        try
        {
            var result = await _telemetry.IngestAsync(device, dto);
            return Ok(new SensorReadingReceivedDto
            {
                Duplicate = result.Duplicate,
                ReceivedAtUtc = result.Reading?.ReceivedAtUtc ?? DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
