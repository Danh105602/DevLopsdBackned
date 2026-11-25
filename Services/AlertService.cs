using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.Sensors;
using WebApplication1.Models;

namespace WebApplication1.Services;

public interface IAlertService
{
    Task<List<AlertRule>> GetRulesAsync(int? ownerId, int? fieldId);
    Task<AlertRule?> GetRuleByIdAsync(int id);
    Task<AlertRule> CreateRuleAsync(AlertRule rule);
    Task<AlertRule?> UpdateRuleAsync(int id, AlertRule values);
    Task<bool> DisableRuleAsync(int id);
    Task<List<Alert>> GetAlertsAsync(int? ownerId, int? farmId, string? status);
    Task<Alert?> GetAlertByIdAsync(int id);
    Task<Alert?> MarkSeenAsync(int id);
    Task<Alert?> ResolveAsync(int id);
    Task<List<Alert>> EvaluateReadingAsync(SensorDevice device, SensorReading reading);
    Task EvaluateOfflineDevicesAsync();
}

public class AlertService : IAlertService
{
    private readonly AppDbContext _context;
    private readonly SensorRealtimePublisher _publisher;

    public AlertService(AppDbContext context, SensorRealtimePublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public Task<List<AlertRule>> GetRulesAsync(int? ownerId, int? fieldId)
    {
        var query = _context.AlertRules.AsNoTracking();
        if (ownerId.HasValue) query = query.Where(r => r.Field!.Farm!.OwnerId == ownerId.Value);
        if (fieldId.HasValue) query = query.Where(r => r.FieldId == fieldId.Value);
        return query.OrderBy(r => r.FieldId).ThenBy(r => r.Metric).ToListAsync();
    }

    public Task<AlertRule?> GetRuleByIdAsync(int id) => _context.AlertRules
        .AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

    public async Task<AlertRule> CreateRuleAsync(AlertRule rule)
    {
        _context.AlertRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<AlertRule?> UpdateRuleAsync(int id, AlertRule values)
    {
        var rule = await _context.AlertRules.FindAsync(id);
        if (rule == null) return null;
        rule.LowerThreshold = values.LowerThreshold;
        rule.UpperThreshold = values.UpperThreshold;
        rule.ConsecutiveReadings = values.ConsecutiveReadings;
        rule.CooldownMinutes = values.CooldownMinutes;
        rule.Severity = values.Severity;
        rule.IsEnabled = values.IsEnabled;
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<bool> DisableRuleAsync(int id)
    {
        var rule = await _context.AlertRules.FindAsync(id);
        if (rule == null) return false;
        rule.IsEnabled = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<Alert>> GetAlertsAsync(int? ownerId, int? farmId, string? status)
    {
        IQueryable<Alert> query = _context.Alerts.AsNoTracking()
            .Include(a => a.SensorDevice).ThenInclude(d => d!.Field);
        if (ownerId.HasValue) query = query.Where(a => a.SensorDevice!.Field!.Farm!.OwnerId == ownerId.Value);
        if (farmId.HasValue) query = query.Where(a => a.SensorDevice!.Field!.FarmId == farmId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
        return query.OrderByDescending(a => a.TriggeredAtUtc).Take(500).ToListAsync();
    }

    public Task<Alert?> GetAlertByIdAsync(int id) => _context.Alerts.AsNoTracking()
        .Include(a => a.SensorDevice).ThenInclude(d => d!.Field)
        .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Alert?> MarkSeenAsync(int id)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert == null) return null;
        if (alert.Status == AlertStatuses.New)
        {
            alert.Status = AlertStatuses.Seen;
            alert.SeenAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return alert;
    }

    public async Task<Alert?> ResolveAsync(int id)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert == null) return null;
        if (alert.Status != AlertStatuses.Resolved)
        {
            alert.Status = AlertStatuses.Resolved;
            alert.ResolvedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return alert;
    }

    public async Task<List<Alert>> EvaluateReadingAsync(SensorDevice device, SensorReading reading)
    {
        var emitted = new List<Alert>();
        var now = DateTime.UtcNow;
        if (reading.MeasuredAtUtc < now.AddMinutes(-10) || reading.MeasuredAtUtc > now.AddMinutes(5))
            return emitted;

        var rules = await _context.AlertRules
            .Where(r => r.FieldId == device.FieldId && r.IsEnabled)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (rule.Metric == SensorMetrics.DeviceOffline) continue;
            var values = await RecentMetricValues(device.Id, reading.MeasuredAtUtc, rule.Metric, rule.ConsecutiveReadings);
            if (values.Count == 0) continue;

            var allOutside = values.Count == rule.ConsecutiveReadings && values.All(v => IsOutside(v, rule));
            var allInside = values.Count == rule.ConsecutiveReadings && values.All(v => !IsOutside(v, rule));
            var active = await _context.Alerts.FirstOrDefaultAsync(a =>
                a.AlertRuleId == rule.Id && a.SensorDeviceId == device.Id && a.Status != AlertStatuses.Resolved);

            if (allOutside)
            {
                if (active == null)
                {
                    var latestValue = values[0];
                    active = new Alert
                    {
                        AlertRuleId = rule.Id,
                        SensorDeviceId = device.Id,
                        SensorReadingId = reading.Id,
                        Severity = rule.Severity,
                        Message = $"{rule.Metric} is outside its configured range. Latest value: {latestValue}.",
                        Status = AlertStatuses.New,
                        TriggeredAtUtc = now,
                        LastNotifiedAtUtc = now
                    };
                    _context.Alerts.Add(active);
                    await _context.SaveChangesAsync();
                    emitted.Add(active);
                }
                else if (now - active.LastNotifiedAtUtc >= TimeSpan.FromMinutes(rule.CooldownMinutes))
                {
                    active.LastNotifiedAtUtc = now;
                    active.SensorReadingId = reading.Id;
                    active.Message = $"{rule.Metric} remains outside its configured range. Latest value: {values[0]}.";
                    await _context.SaveChangesAsync();
                    emitted.Add(active);
                }
            }
            else if (allInside && active != null)
            {
                active.Status = AlertStatuses.Resolved;
                active.ResolvedAtUtc = now;
                await _context.SaveChangesAsync();
                emitted.Add(active);
            }
        }

        return emitted;
    }

    public async Task EvaluateOfflineDevicesAsync()
    {
        var rules = await _context.AlertRules.AsNoTracking()
            .Where(r => r.IsEnabled && r.Metric == SensorMetrics.DeviceOffline && r.UpperThreshold.HasValue)
            .ToListAsync();
        var now = DateTime.UtcNow;

        foreach (var rule in rules)
        {
            var devices = await _context.SensorDevices.AsNoTracking()
                .Include(d => d.Field)
                .Where(d => d.IsActive && d.FieldId == rule.FieldId)
                .ToListAsync();

            foreach (var device in devices)
            {
                var lastSignal = device.LastSeenAt ?? device.CreatedAt;
                var isOffline = now - lastSignal >= TimeSpan.FromMinutes((double)rule.UpperThreshold!.Value);
                var active = await _context.Alerts.FirstOrDefaultAsync(a =>
                    a.AlertRuleId == rule.Id && a.SensorDeviceId == device.Id && a.Status != AlertStatuses.Resolved);

                if (isOffline && active == null)
                {
                    active = new Alert
                    {
                        AlertRuleId = rule.Id,
                        SensorDeviceId = device.Id,
                        Severity = rule.Severity,
                        Message = $"Sensor device {device.Name} has not reported for {rule.UpperThreshold} minutes.",
                        Status = AlertStatuses.New,
                        TriggeredAtUtc = now,
                        LastNotifiedAtUtc = now
                    };
                    _context.Alerts.Add(active);
                    await _context.SaveChangesAsync();
                    _publisher.QueueAlert(device, active);
                }
                else if (isOffline && active != null &&
                         now - active.LastNotifiedAtUtc >= TimeSpan.FromMinutes(rule.CooldownMinutes))
                {
                    active.LastNotifiedAtUtc = now;
                    active.Message = $"Sensor device {device.Name} is still offline.";
                    await _context.SaveChangesAsync();
                    _publisher.QueueAlert(device, active);
                }
                else if (!isOffline && active != null)
                {
                    active.Status = AlertStatuses.Resolved;
                    active.ResolvedAtUtc = now;
                    await _context.SaveChangesAsync();
                    _publisher.QueueAlert(device, active);
                }
            }
        }
    }

    private Task<List<decimal>> RecentMetricValues(int deviceId, DateTime atUtc, string metric, int count)
    {
        var readings = _context.SensorReadings.AsNoTracking()
            .Where(r => r.SensorDeviceId == deviceId && r.MeasuredAtUtc <= atUtc);
        var ordered = readings.OrderByDescending(r => r.MeasuredAtUtc).ThenByDescending(r => r.Id);
        IQueryable<decimal?> values = metric switch
        {
            SensorMetrics.Temperature => ordered.Where(r => r.TemperatureC.HasValue).Select(r => r.TemperatureC),
            SensorMetrics.AirHumidity => ordered.Where(r => r.AirHumidityPercent.HasValue).Select(r => r.AirHumidityPercent),
            SensorMetrics.SoilMoisture => ordered.Where(r => r.SoilMoisturePercent.HasValue).Select(r => r.SoilMoisturePercent),
            SensorMetrics.PH => ordered.Where(r => r.PH.HasValue).Select(r => r.PH),
            _ => ordered.Where(_ => false).Select(_ => (decimal?)null)
        };

        return values.Take(count)
            .Select(v => v!.Value)
            .ToListAsync();
    }

    private static bool IsOutside(decimal value, AlertRule rule) =>
        (rule.LowerThreshold.HasValue && value < rule.LowerThreshold.Value) ||
        (rule.UpperThreshold.HasValue && value > rule.UpperThreshold.Value);
}
