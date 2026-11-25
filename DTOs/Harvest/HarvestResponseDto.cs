
namespace WebApplication1.DTOs.Harvest
{
    public class HarvestResponseDto
    {
        public int Id { get; set; }

        public int PlantingId { get; set; }

        public DateTime HarvestDate { get; set; }

        public double QuantityKg { get; set; }

        public string Quality { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
