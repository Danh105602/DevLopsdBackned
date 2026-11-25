namespace WebApplication1.DTOs.CareSchedule
{
    public class CareScheduleResponseDto
    {
        public int Id { get; set; }
        public int PlantingId { get; set; }
        public string TaskType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsOverdue { get; set; }
    }
}
