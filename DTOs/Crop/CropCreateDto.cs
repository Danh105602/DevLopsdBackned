
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Crop
{
    public class CropCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 3650)]
        public int GrowthDays { get; set; }
    }
}
