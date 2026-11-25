using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public static class PestIncidentSeverities
    {
        public const string Low = "Thấp";
        public const string Medium = "Trung bình";
        public const string High = "Cao";

        public static bool IsValid(string? severity) =>
            severity is Low or Medium or High;
    }

    public static class PestIncidentStatuses
    {
        public const string Open = "Open";
        public const string Treated = "Treated";
        public const string Resolved = "Resolved";

        public static bool IsValid(string? status) =>
            status is Open or Treated or Resolved;
    }

    public class PestIncident
    {
        public int Id { get; set; }

        public int PlantingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PestName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = PestIncidentSeverities.Medium;

        public DateTime DetectedDate { get; set; }

        [MaxLength(1000)]
        public string Treatment { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = PestIncidentStatuses.Open;

        public DateTime? ResolutionDate { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Planting? Planting { get; set; }
    }
}
