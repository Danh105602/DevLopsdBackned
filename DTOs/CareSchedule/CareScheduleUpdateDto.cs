using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.CareSchedule
{
    public class CareScheduleUpdateDto
    {
        [Required]
        [MaxLength(50)]
        public string TaskType { get; set; } = string.Empty;

        public DateTime ScheduledDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
