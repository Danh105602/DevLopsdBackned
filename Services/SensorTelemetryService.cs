using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.Sensors;
using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed record SensorIngestResult(SensorReading? Reading, bool Duplicate, List<Alert> Alerts);

public interface ISensorTelemetryService
{
    Task<SensorIngestResult> IngestAsync(SensorDevice device, SensorReadingIngestDto input);
    Task<List<SensorHistoryPointDto>> GetHistoryAsync(int deviceId, DateTime fromUtc, DateTime toUtc, string bucket);
}

public class SensorTelemetryService : ISensorTelemetryService
{
    private const int RawRetentionDays = 90;
    private readonly AppDbContext _context;
    private readonly IAlertService _alerts;
    private readonly SensorRealtimePublisher _publisher;

    public SensorTelemetryService(AppDbContext context, IAlertService alerts, SensorRealtimePublisher publisher)
    {
        _context = context;
        _alerts = alerts;
        _publisher = publisher;
    }

    public async Task<SensorIngestResult> IngestAsync(SensorDevice device, SensorReadingIngestDto input)
    {
        if (input.TemperatureC == null && input.AirHumidityPercent == null &&
            input.SoilMoisturePercent == null && input.PH == null)
            throw new ArgumentException("At least one sensor value is required.");

        var now = DateTime.UtcNow;
        var measuredAt = input.MeasuredAtUtc.Kind switch
        {
            DateTimeKind.Utc => input.MeasuredAtUtc,
            DateTimeKind.Local => input.MeasuredAtUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(input.MeasuredAtUtc, DateTimeKind.Utc)
        };
        if (measuredAt > now.AddMinutes(5) || measuredAt < now.AddDays(-365))
            throw new ArgumentException("MeasuredAtUtc must be within the last 365 days and no more than 5 minutes in the future.");

        await _context.SensorDevices.Where(d => d.Id == device.Id && d.IsActive)
            .ExecuteUpdateAsync(update => update.SetProperty(d => d.LastSeenAt, now));

        var duplicate = await _context.SensorReadings.AsNoTracking()
            .AnyAsync(r => r.SensorDeviceId == device.Id && r.MessageId == input.MessageId);
        if (duplicate)
            return new SensorIngestResult(null, true, []);

        var reading = new SensorReading
        {
            SensorDeviceId = device.Id,
            MessageId = input.MessageId,
            MeasuredAtUtc = measuredAt,
            ReceivedAtUtc = now,
            TemperatureC = input.TemperatureC,
            AirHumidityPercent = input.AirHumidityPercent,
            SoilMoisturePercent = input.SoilMoisturePercent,
            PH = input.PH
        };
        _context.SensorReadings.Add(reading);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _context.Entry(reading).State = EntityState.Detached;
            if (await _context.SensorReadings.AsNoTracking()
                .AnyAsync(r => r.SensorDeviceId == device.Id && r.MessageId == input.MessageId))
                return new SensorIngestResult(null, true, []);
            throw;
        }

        var emittedAlerts = await _alerts.EvaluateReadingAsync(device, reading);
        _publisher.QueueReading(device, reading);
        foreach (var alert in emittedAlerts) _publisher.QueueAlert(device, alert);
        return new SensorIngestResult(reading, false, emittedAlerts);
    }

    public async Task<List<SensorHistoryPointDto>> GetHistoryAsync(
        int deviceId, DateTime fromUtc, DateTime toUtc, string bucket)
    {
        var byHour = string.Equals(bucket, "hour", StringComparison.OrdinalIgnoreCase);
        var cutoffRaw = DateTime.UtcNow.AddDays(-RawRetentionDays);
        var rawCutoff = new DateTime(cutoffRaw.Year, cutoffRaw.Month, cutoffRaw.Day, cutoffRaw.Hour, 0, 0, DateTimeKind.Utc);
        var combined = new Dictionary<DateTime, HistoryAccumulator>();

        var raw = _context.SensorReadings.AsNoTracking()
            .Where(r => r.SensorDeviceId == deviceId && r.MeasuredAtUtc >= rawCutoff &&
                        r.MeasuredAtUtc >= fromUtc && r.MeasuredAtUtc < toUtc);
        var rawParts = await raw
            .GroupBy(r => new
            {
                LocalYear = r.MeasuredAtUtc.AddHours(7).Year,
                LocalMonth = r.MeasuredAtUtc.AddHours(7).Month,
                LocalDay = r.MeasuredAtUtc.AddHours(7).Day,
                LocalHour = byHour ? r.MeasuredAtUtc.AddHours(7).Hour : 0
            })
            .Select(g => new HistoryPart
            {
                Year = g.Key.LocalYear, Month = g.Key.LocalMonth, Day = g.Key.LocalDay, Hour = g.Key.LocalHour,
                ReadingCount = g.Count(),
                TemperatureCount = g.Count(r => r.TemperatureC.HasValue),
                TemperatureAverage = g.Average(r => r.TemperatureC),
                TemperatureMinimum = g.Min(r => r.TemperatureC),
                TemperatureMaximum = g.Max(r => r.TemperatureC),
                AirHumidityCount = g.Count(r => r.AirHumidityPercent.HasValue),
                AirHumidityAverage = g.Average(r => r.AirHumidityPercent),
                AirHumidityMinimum = g.Min(r => r.AirHumidityPercent),
                AirHumidityMaximum = g.Max(r => r.AirHumidityPercent),
                SoilMoistureCount = g.Count(r => r.SoilMoisturePercent.HasValue),
                SoilMoistureAverage = g.Average(r => r.SoilMoisturePercent),
                SoilMoistureMinimum = g.Min(r => r.SoilMoisturePercent),
                SoilMoistureMaximum = g.Max(r => r.SoilMoisturePercent),
                PHCount = g.Count(r => r.PH.HasValue),
                PHAverage = g.Average(r => r.PH),
                PHMinimum = g.Min(r => r.PH),
                PHMaximum = g.Max(r => r.PH)
            })
            .ToListAsync();
        foreach (var part in rawParts) Add(combined, part);

        var archived = _context.SensorReadingAggregates.AsNoTracking()
            .Where(a => a.SensorDeviceId == deviceId && a.BucketStartUtc < rawCutoff &&
                        a.BucketStartUtc >= fromUtc && a.BucketStartUtc < toUtc);
        var archiveParts = await archived
            .GroupBy(a => new
            {
                LocalYear = a.BucketStartUtc.AddHours(7).Year,
                LocalMonth = a.BucketStartUtc.AddHours(7).Month,
                LocalDay = a.BucketStartUtc.AddHours(7).Day,
                LocalHour = byHour ? a.BucketStartUtc.AddHours(7).Hour : 0
            })
            .Select(g => new HistoryPart
            {
                Year = g.Key.LocalYear, Month = g.Key.LocalMonth, Day = g.Key.LocalDay, Hour = g.Key.LocalHour,
                ReadingCount = g.Sum(a => a.ReadingCount),
                TemperatureCount = g.Sum(a => a.TemperatureCount),
                TemperatureAverage = g.Sum(a => a.TemperatureCount) == 0 ? null :
                    g.Sum(a => a.TemperatureAverage * a.TemperatureCount) / g.Sum(a => a.TemperatureCount),
                TemperatureMinimum = g.Min(a => a.TemperatureMinimum),
                TemperatureMaximum = g.Max(a => a.TemperatureMaximum),
                AirHumidityCount = g.Sum(a => a.AirHumidityCount),
                AirHumidityAverage = g.Sum(a => a.AirHumidityCount) == 0 ? null :
                    g.Sum(a => a.AirHumidityAverage * a.AirHumidityCount) / g.Sum(a => a.AirHumidityCount),
                AirHumidityMinimum = g.Min(a => a.AirHumidityMinimum),
                AirHumidityMaximum = g.Max(a => a.AirHumidityMaximum),
                SoilMoistureCount = g.Sum(a => a.SoilMoistureCount),
                SoilMoistureAverage = g.Sum(a => a.SoilMoistureCount) == 0 ? null :
                    g.Sum(a => a.SoilMoistureAverage * a.SoilMoistureCount) / g.Sum(a => a.SoilMoistureCount),
                SoilMoistureMinimum = g.Min(a => a.SoilMoistureMinimum),
                SoilMoistureMaximum = g.Max(a => a.SoilMoistureMaximum),
                PHCount = g.Sum(a => a.PHCount),
                PHAverage = g.Sum(a => a.PHCount) == 0 ? null :
                    g.Sum(a => a.PHAverage * a.PHCount) / g.Sum(a => a.PHCount),
                PHMinimum = g.Min(a => a.PHMinimum),
                PHMaximum = g.Max(a => a.PHMaximum)
            })
            .ToListAsync();
        foreach (var part in archiveParts) Add(combined, part);

        return combined.OrderBy(kv => kv.Key).Select(kv => kv.Value.ToDto(kv.Key)).ToList();
    }

    private static void Add(Dictionary<DateTime, HistoryAccumulator> combined, HistoryPart part)
    {
        var bucket = new DateTime(part.Year, part.Month, part.Day, part.Hour, 0, 0, DateTimeKind.Unspecified);
        if (!combined.TryGetValue(bucket, out var acc)) combined[bucket] = acc = new HistoryAccumulator();
        acc.ReadingCount += part.ReadingCount;
        acc.Temperature.Add(part.TemperatureCount, part.TemperatureAverage, part.TemperatureMinimum, part.TemperatureMaximum);
        acc.AirHumidity.Add(part.AirHumidityCount, part.AirHumidityAverage, part.AirHumidityMinimum, part.AirHumidityMaximum);
        acc.SoilMoisture.Add(part.SoilMoistureCount, part.SoilMoistureAverage, part.SoilMoistureMinimum, part.SoilMoistureMaximum);
        acc.PH.Add(part.PHCount, part.PHAverage, part.PHMinimum, part.PHMaximum);
    }

    private sealed class HistoryPart
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int ReadingCount { get; set; }
        public int TemperatureCount { get; set; }
        public decimal? TemperatureAverage { get; set; }
        public decimal? TemperatureMinimum { get; set; }
        public decimal? TemperatureMaximum { get; set; }
        public int AirHumidityCount { get; set; }
        public decimal? AirHumidityAverage { get; set; }
        public decimal? AirHumidityMinimum { get; set; }
        public decimal? AirHumidityMaximum { get; set; }
        public int SoilMoistureCount { get; set; }
        public decimal? SoilMoistureAverage { get; set; }
        public decimal? SoilMoistureMinimum { get; set; }
        public decimal? SoilMoistureMaximum { get; set; }
        public int PHCount { get; set; }
        public decimal? PHAverage { get; set; }
        public decimal? PHMinimum { get; set; }
        public decimal? PHMaximum { get; set; }
    }

    private sealed class MetricAccumulator
    {
        private decimal _weightedTotal;
        public int Count { get; private set; }
        public decimal? Minimum { get; private set; }
        public decimal? Maximum { get; private set; }
        public decimal? Average => Count == 0 ? null : _weightedTotal / Count;

        public void Add(int count, decimal? average, decimal? min, decimal? max)
        {
            if (count == 0 || average == null) return;
            Count += count;
            _weightedTotal += average.Value * count;
            Minimum = Minimum.HasValue && min.HasValue ? Math.Min(Minimum.Value, min.Value) : Minimum ?? min;
            Maximum = Maximum.HasValue && max.HasValue ? Math.Max(Maximum.Value, max.Value) : Maximum ?? max;
        }
    }

    private sealed class HistoryAccumulator
    {
        public int ReadingCount;
        public MetricAccumulator Temperature { get; } = new();
        public MetricAccumulator AirHumidity { get; } = new();
        public MetricAccumulator SoilMoisture { get; } = new();
        public MetricAccumulator PH { get; } = new();

        public SensorHistoryPointDto ToDto(DateTime localBucket)
        {
            var offset = TimeSpan.FromHours(7);
            return new SensorHistoryPointDto
            {
                BucketStart = new DateTimeOffset(DateTime.SpecifyKind(localBucket, DateTimeKind.Unspecified), offset),
                ReadingCount = ReadingCount,
                TemperatureAverage = Temperature.Average,
                TemperatureMinimum = Temperature.Minimum,
                TemperatureMaximum = Temperature.Maximum,
                AirHumidityAverage = AirHumidity.Average,
                AirHumidityMinimum = AirHumidity.Minimum,
                AirHumidityMaximum = AirHumidity.Maximum,
                SoilMoistureAverage = SoilMoisture.Average,
                SoilMoistureMinimum = SoilMoisture.Minimum,
                SoilMoistureMaximum = SoilMoisture.Maximum,
                PHAverage = PH.Average,
                PHMinimum = PH.Minimum,
                PHMaximum = PH.Maximum
            };
        }
    }
}
