using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public static class SensorMetrics
{
    public const string Temperature = "Temperature";
    public const string AirHumidity = "AirHumidity";
    public const string SoilMoisture = "SoilMoisture";
    public const string PH = "PH";
    public const string DeviceOffline = "DeviceOffline";

    public static bool IsValid(string? value) => value is Temperature or AirHumidity or SoilMoisture or PH or DeviceOffline;
}

public static class AlertSeverities
{
    public const string Normal = "Normal";
    public const string Urgent = "Urgent";
}

public static class AlertStatuses
{
    public const string New = "New";
    public const string Seen = "Seen";
    public const string Resolved = "Resolved";
}

public class AlertRule
{
    public int Id { get; set; }
    public int FieldId { get; set; }

    [Required, MaxLength(30)]
    public string Metric { get; set; } = string.Empty;

    public decimal? LowerThreshold { get; set; }
    public decimal? UpperThreshold { get; set; }
    public int ConsecutiveReadings { get; set; } = 2;
    public int CooldownMinutes { get; set; } = 180;

    [Required, MaxLength(20)]
    public string Severity { get; set; } = AlertSeverities.Normal;

    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Field? Field { get; set; }
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
