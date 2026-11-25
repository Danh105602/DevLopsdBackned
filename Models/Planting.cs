using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Planting
    {
        public int Id { get; set; }

        public int FieldId { get; set; }

        public int CropId { get; set; }

        public DateTime PlantingDate { get; set; }

        public DateTime? ExpectedHarvestDate { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "Growing";

        // Navigation
        public Field? Field { get; set; } = null!;

        public Crop? Crop { get; set; } = null!;

        public ICollection<CareSchedule> CareSchedules { get; set; } = new List<CareSchedule>();

        public ICollection<PestIncident> PestIncidents { get; set; } = new List<PestIncident>();
    }
}
