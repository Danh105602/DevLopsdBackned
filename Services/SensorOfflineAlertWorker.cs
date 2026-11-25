namespace WebApplication1.Services;

public class SensorOfflineAlertWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorOfflineAlertWorker> _logger;

    public SensorOfflineAlertWorker(IServiceScopeFactory scopeFactory, ILogger<SensorOfflineAlertWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IAlertService>().EvaluateOfflineDevicesAsync();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sensor offline alert check failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
