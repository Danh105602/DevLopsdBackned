using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Sensors;

public class SensorDeviceCreateDto
{
    [Range(1, int.MaxValue)]
    public int FieldId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class SensorDeviceUpdateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class SensorDeviceDto
{
    public int Id { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int FieldId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class ProvisionedSensorDeviceDto : SensorDeviceDto
{
    public string ApiKey { get; set; } = string.Empty;
}
