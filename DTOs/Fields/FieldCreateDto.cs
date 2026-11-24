using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Fields
{
    public class FieldCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int FarmId { get; set; }

        [MaxLength(50)]
        public string SoilType { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public double Area { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}