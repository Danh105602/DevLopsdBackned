namespace WebApplication1.Models;

public class SensorReadingAggregate
{
    public long Id { get; set; }
    public int SensorDeviceId { get; set; }
    public DateTime BucketStartUtc { get; set; }
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

    public SensorDevice? SensorDevice { get; set; }
}
