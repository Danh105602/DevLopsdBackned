using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Harvest
    {
        public int Id { get; set; }

        public int PlantingId { get; set; }

        public DateTime HarvestDate { get; set; }

        // Sản lượng (kg)
        public double QuantityKg { get; set; }

        [MaxLength(30)]
        public string Quality { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Planting? Planting { get; set; }
    }
}