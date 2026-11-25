using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Sensors;

public class SensorReadingIngestDto
{
    [Required, MaxLength(80)]
    public string DeviceCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string MessageId { get; set; } = string.Empty;

    [Required]
    public DateTime MeasuredAtUtc { get; set; }

    [Range(typeof(decimal), "-50", "80")]
    public decimal? TemperatureC { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? AirHumidityPercent { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? SoilMoisturePercent { get; set; }

    [Range(typeof(decimal), "0", "14")]
    public decimal? PH { get; set; }
}

public class SensorReadingReceivedDto
{
    public bool Duplicate { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
}

public class SensorHistoryPointDto
{
    public DateTimeOffset BucketStart { get; set; }
    public int ReadingCount { get; set; }
    public decimal? TemperatureAverage { get; set; }
    public decimal? TemperatureMinimum { get; set; }
    public decimal? TemperatureMaximum { get; set; }
    public decimal? AirHumidityAverage { get; set; }
    public decimal? AirHumidityMinimum { get; set; }
    public decimal? AirHumidityMaximum { get; set; }
    public decimal? SoilMoistureAverage { get; set; }
    public decimal? SoilMoistureMinimum { get; set; }
    public decimal? SoilMoistureMaximum { get; set; }
    public decimal? PHAverage { get; set; }
    public decimal? PHMinimum { get; set; }
    public decimal? PHMaximum { get; set; }
}
