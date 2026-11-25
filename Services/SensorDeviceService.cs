using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.Sensors;
using WebApplication1.Models;

namespace WebApplication1.Services;

public interface ISensorDeviceService
{
    Task<List<SensorDevice>> GetAllAsync(int? ownerId, int? farmId);
    Task<SensorDevice?> GetByIdAsync(int id);
    Task<(SensorDevice Device, string ApiKey)> CreateAsync(int fieldId, string name);
    Task<SensorDevice?> UpdateNameAsync(int id, string name);
    Task<(SensorDevice? Device, string? ApiKey)> RotateKeyAsync(int id);
    Task<SensorDevice?> RevokeAsync(int id);
    Task<SensorDevice?> AuthenticateAsync(string deviceCode, string apiKey);
}

public class SensorDeviceService : ISensorDeviceService
{
    private readonly AppDbContext _context;

    public SensorDeviceService(AppDbContext context) => _context = context;

    public Task<List<SensorDevice>> GetAllAsync(int? ownerId, int? farmId)
    {
        var query = _context.SensorDevices.AsNoTracking();
        if (ownerId.HasValue) query = query.Where(d => d.Field!.Farm!.OwnerId == ownerId.Value);
        if (farmId.HasValue) query = query.Where(d => d.Field!.FarmId == farmId.Value);
        return query.OrderBy(d => d.Name).ToListAsync();
    }

    public Task<SensorDevice?> GetByIdAsync(int id) => _context.SensorDevices
        .AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<(SensorDevice Device, string ApiKey)> CreateAsync(int fieldId, string name)
    {
        var apiKey = GenerateApiKey();
        var device = new SensorDevice
        {
            DeviceCode = Guid.NewGuid().ToString("N"),
            Name = name.Trim(),
            FieldId = fieldId,
            ApiKeyHash = HashApiKey(apiKey),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.SensorDevices.Add(device);
        await _context.SaveChangesAsync();
        return (device, apiKey);
    }

    public async Task<SensorDevice?> UpdateNameAsync(int id, string name)
    {
        var device = await _context.SensorDevices.FindAsync(id);
        if (device == null) return null;
        device.Name = name.Trim();
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<(SensorDevice? Device, string? ApiKey)> RotateKeyAsync(int id)
    {
        var device = await _context.SensorDevices.FindAsync(id);
        if (device == null) return (null, null);
        if (!device.IsActive) return (device, null);
        var apiKey = GenerateApiKey();
        device.ApiKeyHash = HashApiKey(apiKey);
        await _context.SaveChangesAsync();
        return (device, apiKey);
    }

    public async Task<SensorDevice?> RevokeAsync(int id)
    {
        var device = await _context.SensorDevices.FindAsync(id);
        if (device == null) return null;
        if (device.IsActive)
        {
            device.IsActive = false;
            device.RevokedAt = DateTime.UtcNow;
            device.ApiKeyHash = HashApiKey(GenerateApiKey());
            await _context.SaveChangesAsync();
        }
        return device;
    }

    public Task<SensorDevice?> AuthenticateAsync(string deviceCode, string apiKey)
    {
        var hash = HashApiKey(apiKey);
        return _context.SensorDevices.AsNoTracking()
            .Include(d => d.Field)
            .FirstOrDefaultAsync(d => d.DeviceCode == deviceCode && d.ApiKeyHash == hash && d.IsActive);
    }

    public static string GenerateApiKey() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string HashApiKey(string apiKey) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));
}

public sealed class SensorDeviceRateLimiter : IDisposable
{
    private readonly ConcurrentDictionary<int, TokenBucketRateLimiter> _limiters = new();

    public bool TryAcquire(int deviceId)
    {
        var limiter = _limiters.GetOrAdd(deviceId, _ => new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 120,
                TokensPerPeriod = 60,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                AutoReplenishment = true,
                QueueLimit = 0
            }));
        using var lease = limiter.AttemptAcquire();
        return lease.IsAcquired;
    }

    public void Dispose()
    {
        foreach (var limiter in _limiters.Values) limiter.Dispose();
    }
}
