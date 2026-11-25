using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services;

public interface ISensorMaintenanceService
{
    Task<int> RollupExpiredReadingsAsync(CancellationToken cancellationToken);
}

public class SensorMaintenanceService : ISensorMaintenanceService
{
    private readonly AppDbContext _context;

    public SensorMaintenanceService(AppDbContext context) => _context = context;

    public async Task<int> RollupExpiredReadingsAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-90);
        cutoff = new DateTime(cutoff.Year, cutoff.Month, cutoff.Day, cutoff.Hour, 0, 0, DateTimeKind.Utc);
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var batches = await _context.SensorReadings.AsNoTracking()
            .Where(r => r.MeasuredAtUtc < cutoff)
            .GroupBy(r => new
            {
                r.SensorDeviceId,
                r.MeasuredAtUtc.Year,
                r.MeasuredAtUtc.Month,
                r.MeasuredAtUtc.Day,
                r.MeasuredAtUtc.Hour
            })
            .Select(g => new HourlyRollup
            {
                SensorDeviceId = g.Key.SensorDeviceId,
                Year = g.Key.Year,
                Month = g.Key.Month,
                Day = g.Key.Day,
                Hour = g.Key.Hour,
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
            .ToListAsync(cancellationToken);

        _context.SensorReadingAggregates.AddRange(batches.Select(b => new SensorReadingAggregate
        {
            SensorDeviceId = b.SensorDeviceId,
            BucketStartUtc = new DateTime(b.Year, b.Month, b.Day, b.Hour, 0, 0, DateTimeKind.Utc),
            ReadingCount = b.ReadingCount,
            TemperatureCount = b.TemperatureCount,
            TemperatureAverage = b.TemperatureAverage,
            TemperatureMinimum = b.TemperatureMinimum,
            TemperatureMaximum = b.TemperatureMaximum,
            AirHumidityCount = b.AirHumidityCount,
            AirHumidityAverage = b.AirHumidityAverage,
            AirHumidityMinimum = b.AirHumidityMinimum,
            AirHumidityMaximum = b.AirHumidityMaximum,
            SoilMoistureCount = b.SoilMoistureCount,
            SoilMoistureAverage = b.SoilMoistureAverage,
            SoilMoistureMinimum = b.SoilMoistureMinimum,
            SoilMoistureMaximum = b.SoilMoistureMaximum,
            PHCount = b.PHCount,
            PHAverage = b.PHAverage,
            PHMinimum = b.PHMinimum,
            PHMaximum = b.PHMaximum
        }));
        await _context.SaveChangesAsync(cancellationToken);

        var deleted = await _context.SensorReadings
            .Where(r => r.MeasuredAtUtc < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return deleted;
    }

    private sealed class HourlyRollup
    {
        public int SensorDeviceId { get; set; }
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
}

public class SensorRetentionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorRetentionWorker> _logger;

    public SensorRetentionWorker(IServiceScopeFactory scopeFactory, ILogger<SensorRetentionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var maintenance = scope.ServiceProvider.GetRequiredService<ISensorMaintenanceService>();
                var count = await maintenance.RollupExpiredReadingsAsync(stoppingToken);
                if (count > 0) _logger.LogInformation("Rolled up and removed {ReadingCount} expired sensor readings", count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sensor reading retention task failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
