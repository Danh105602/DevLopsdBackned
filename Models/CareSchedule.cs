using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public static class CareScheduleStatuses
    {
        public const string Pending = "Chờ làm";
        public const string Completed = "Đã làm";
        public const string Skipped = "Bỏ qua";

        public static bool IsValid(string? status) =>
            status is Pending or Completed or Skipped;
    }

    public static class CareScheduleTaskTypes
    {
        public static readonly IReadOnlyCollection<string> All =
        ["Tưới", "Bón phân", "Phun thuốc", "Làm cỏ", "Tỉa cành"];

        public static bool IsValid(string? taskType) =>
            !string.IsNullOrWhiteSpace(taskType) &&
            All.Contains(taskType.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public class CareSchedule
    {
        public int Id { get; set; }

        public int PlantingId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaskType { get; set; } = string.Empty;

        public DateTime ScheduledDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = CareScheduleStatuses.Pending;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Planting? Planting { get; set; }
    }
}
