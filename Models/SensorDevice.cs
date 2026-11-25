using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class SensorDevice
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string DeviceCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int FieldId { get; set; }

    [Required, MaxLength(64)]
    public string ApiKeyHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public Field? Field { get; set; }
    public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>();
}
