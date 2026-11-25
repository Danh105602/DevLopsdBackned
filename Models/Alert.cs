using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Alert
{
    public int Id { get; set; }
    public int AlertRuleId { get; set; }
    public int SensorDeviceId { get; set; }
    public long? SensorReadingId { get; set; }

    [Required, MaxLength(20)]
    public string Severity { get; set; } = AlertSeverities.Normal;

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = AlertStatuses.New;

    public DateTime TriggeredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastNotifiedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SeenAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }

    public AlertRule? AlertRule { get; set; }
    public SensorDevice? SensorDevice { get; set; }
    public SensorReading? SensorReading { get; set; }
}
