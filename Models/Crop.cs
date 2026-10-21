using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Crop
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public int GrowthDays { get; set; }

        // Navigation
        public ICollection<Planting> Plantings { get; set; } = new List<Planting>();
    }
}