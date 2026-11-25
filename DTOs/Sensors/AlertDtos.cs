using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Sensors;

public class AlertRuleCreateDto
{
    [Range(1, int.MaxValue)]
    public int FieldId { get; set; }

    [Required, MaxLength(30)]
    public string Metric { get; set; } = string.Empty;

    public decimal? LowerThreshold { get; set; }
    public decimal? UpperThreshold { get; set; }

    [Range(2, 10)]
    public int ConsecutiveReadings { get; set; } = 2;

    [Range(1, 1440)]
    public int CooldownMinutes { get; set; } = 180;

    [Required, MaxLength(20)]
    public string Severity { get; set; } = "Normal";
}

public class AlertRuleUpdateDto
{
    public decimal? LowerThreshold { get; set; }
    public decimal? UpperThreshold { get; set; }

    [Range(2, 10)]
    public int ConsecutiveReadings { get; set; } = 2;

    [Range(1, 1440)]
    public int CooldownMinutes { get; set; } = 180;

    [Required, MaxLength(20)]
    public string Severity { get; set; } = "Normal";

    public bool IsEnabled { get; set; } = true;
}

public class AlertRuleDto
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string Metric { get; set; } = string.Empty;
    public decimal? LowerThreshold { get; set; }
    public decimal? UpperThreshold { get; set; }
    public int ConsecutiveReadings { get; set; }
    public int CooldownMinutes { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class AlertDto
{
    public int Id { get; set; }
    public int AlertRuleId { get; set; }
    public int SensorDeviceId { get; set; }
    public long? SensorReadingId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TriggeredAtUtc { get; set; }
    public DateTime? SeenAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public int FieldId { get; set; }
}
