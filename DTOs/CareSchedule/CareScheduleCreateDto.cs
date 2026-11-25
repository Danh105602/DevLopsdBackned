using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.CareSchedule
{
    public class CareScheduleCreateDto
    {
        [Range(1, int.MaxValue)]
        public int PlantingId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaskType { get; set; } = string.Empty;

        public DateTime ScheduledDate { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
