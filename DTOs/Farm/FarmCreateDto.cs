using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Farm
{
    public class FarmCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public double Area { get; set; }

        [Range(1, int.MaxValue)]
        public int OwnerId { get; set; }
    }
}