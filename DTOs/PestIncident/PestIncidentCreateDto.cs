using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.PestIncident
{
    public class PestIncidentCreateDto
    {
        [Range(1, int.MaxValue)]
        public int PlantingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PestName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = string.Empty;

        public DateTime DetectedDate { get; set; }

        [MaxLength(1000)]
        public string Treatment { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
    }
}
