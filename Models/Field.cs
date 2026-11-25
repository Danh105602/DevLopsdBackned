using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Field
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int FarmId { get; set; }

        [MaxLength(50)]
        public string SoilType { get; set; } = string.Empty;

        public double Area { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation
        public Farm? Farm { get; set; } = null!;

        public ICollection<Planting> Plantings { get; set; } = new List<Planting>();

        public ICollection<SensorDevice> SensorDevices { get; set; } = new List<SensorDevice>();

        public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
    }
}
