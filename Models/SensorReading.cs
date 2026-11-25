using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class SensorReading
{
    public long Id { get; set; }
    public int SensorDeviceId { get; set; }

    [Required, MaxLength(100)]
    public string MessageId { get; set; } = string.Empty;

    public DateTime MeasuredAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal? TemperatureC { get; set; }
    public decimal? AirHumidityPercent { get; set; }
    public decimal? SoilMoisturePercent { get; set; }
    public decimal? PH { get; set; }

    public SensorDevice? SensorDevice { get; set; }
}
