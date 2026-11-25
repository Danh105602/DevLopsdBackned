using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using WebApplication1.DTOs.Sensors;
using WebApplication1.Hubs;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class SensorRealtimePublisher : BackgroundService
{
    private sealed record Event(int FarmId, string Name, int DeviceId, long Key, object Payload);
    private readonly ConcurrentQueue<Event> _queue = new();
    private readonly IHubContext<FarmHub> _hub;
    private readonly ILogger<SensorRealtimePublisher> _logger;

    public SensorRealtimePublisher(IHubContext<FarmHub> hub, ILogger<SensorRealtimePublisher> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public void QueueReading(SensorDevice device, SensorReading reading)
    {
        if (device.Field == null) return;
        _queue.Enqueue(new Event(device.Field.FarmId, "SensorReadings", device.Id, reading.Id, new
        {
            reading.Id,
            SensorDeviceId = device.Id,
            reading.MessageId,
            reading.MeasuredAtUtc,
            reading.ReceivedAtUtc,
            reading.TemperatureC,
            reading.AirHumidityPercent,
            reading.SoilMoisturePercent,
            reading.PH
        }));
    }

    public void QueueAlert(SensorDevice device, Alert alert)
    {
        if (device.Field == null) return;
        _queue.Enqueue(new Event(device.Field.FarmId, "Alerts", device.Id, alert.Id, new AlertDto
        {
            Id = alert.Id,
            AlertRuleId = alert.AlertRuleId,
            SensorDeviceId = device.Id,
            SensorReadingId = alert.SensorReadingId,
            Severity = alert.Severity,
            Message = alert.Message,
            Status = alert.Status,
            TriggeredAtUtc = alert.TriggeredAtUtc,
            SeenAtUtc = alert.SeenAtUtc,
            ResolvedAtUtc = alert.ResolvedAtUtc,
            FieldId = device.FieldId
        }));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var batch = new List<Event>();
            while (_queue.TryDequeue(out var item)) batch.Add(item);
            foreach (var farmBatch in batch.GroupBy(e => e.FarmId))
            {
                foreach (var namedBatch in farmBatch.GroupBy(e => e.Name))
                {
                    var latest = namedBatch
                        .GroupBy(e => namedBatch.Key == "SensorReadings" ? e.DeviceId : (int)e.Key)
                        .Select(g => g.Last().Payload)
                        .ToList();
                    try
                    {
                        await _hub.Clients.Group(FarmHub.FarmGroup(farmBatch.Key))
                            .SendAsync(namedBatch.Key, latest, stoppingToken);
                    }
                    catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning(ex, "Could not publish sensor updates for farm {FarmId}", farmBatch.Key);
                    }
                }
            }
        }
    }
}
