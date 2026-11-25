using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Authorization;
using WebApplication1.DTOs.Sensors;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/sensor-devices")]
[Authorize(Policy = Permissions.SensorDeviceRead)]
public class SensorDevicesController : ControllerBase
{
    private readonly ISensorDeviceService _devices;
    private readonly IOwnershipService _ownership;

    public SensorDevicesController(ISensorDeviceService devices, IOwnershipService ownership)
    {
        _devices = devices;
        _ownership = ownership;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? farmId)
    {
        int? ownerId = null;
        if (!User.IsInRole("Admin"))
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            if (farmId.HasValue && !await _ownership.OwnsFarmAsync(farmId.Value, userId))
                return Forbid();
            ownerId = userId;
        }
        var devices = await _devices.GetAllAsync(ownerId, farmId);
        return Ok(devices.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var device = await _devices.GetByIdAsync(id);
        if (device == null) return NotFound();
        if (!await CanAccessField(device.FieldId)) return Forbid();
        return Ok(Map(device));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.SensorDeviceCreate)]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] SensorDeviceCreateDto dto)
    {
        if (!await _ownership.FieldExistsAsync(dto.FieldId)) return NotFound(new { message = "Field not found." });
        var (device, key) = await _devices.CreateAsync(dto.FieldId, dto.Name);
        var response = MapProvisioned(device, key);
        return CreatedAtAction(nameof(GetById), new { id = device.Id }, response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.SensorDeviceUpdate)]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] SensorDeviceUpdateDto dto)
    {
        var device = await _devices.UpdateNameAsync(id, dto.Name);
        return device == null ? NotFound() : Ok(Map(device));
    }

    [HttpPost("{id:int}/rotate-key")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> RotateKey(int id)
    {
        var (device, key) = await _devices.RotateKeyAsync(id);
        if (device == null) return NotFound();
        if (key == null) return Conflict(new { message = "Revoked devices cannot rotate keys." });
        return Ok(MapProvisioned(device, key));
    }

    [HttpPost("{id:int}/revoke")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Revoke(int id)
    {
        var device = await _devices.RevokeAsync(id);
        return device == null ? NotFound() : Ok(Map(device));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.SensorDeviceDelete)]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var device = await _devices.RevokeAsync(id);
        return device == null ? NotFound() : Ok(new { message = "Device deactivated; telemetry history was retained." });
    }

    private async Task<bool> CanAccessField(int fieldId)
    {
        if (User.IsInRole("Admin")) return true;
        return TryGetUserId(out var userId) && await _ownership.OwnsFieldAsync(fieldId, userId);
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private static SensorDeviceDto Map(SensorDevice device) => new()
    {
        Id = device.Id,
        DeviceCode = device.DeviceCode,
        Name = device.Name,
        FieldId = device.FieldId,
        IsActive = device.IsActive,
        CreatedAt = device.CreatedAt,
        LastSeenAt = device.LastSeenAt,
        RevokedAt = device.RevokedAt
    };

    private static ProvisionedSensorDeviceDto MapProvisioned(SensorDevice device, string key) => new()
    {
        Id = device.Id,
        DeviceCode = device.DeviceCode,
        Name = device.Name,
        FieldId = device.FieldId,
        IsActive = device.IsActive,
        CreatedAt = device.CreatedAt,
        LastSeenAt = device.LastSeenAt,
        RevokedAt = device.RevokedAt,
        ApiKey = key
    };
}
