
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Harvest
{
    public class HarvestUpdateDto
    {
        [Required]
        public DateTime HarvestDate { get; set; }

        [Range(0.01, 100000)]
        public double QuantityKg { get; set; }

        [MaxLength(30)]
        public string Quality { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
