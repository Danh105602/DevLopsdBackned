namespace WebApplication1.DTOs.PestIncident
{
    public class PestIncidentResponseDto
    {
        public int Id { get; set; }
        public int PlantingId { get; set; }
        public string PestName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime DetectedDate { get; set; }
        public string Treatment { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ResolutionDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
