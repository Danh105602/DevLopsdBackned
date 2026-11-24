using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Plantings
{
    public class PlantingCreateDto
    {
        [Range(1, int.MaxValue)]
        public int FieldId { get; set; }

        [Range(1, int.MaxValue)]
        public int CropId { get; set; }

        [Required]
        public DateTime PlantingDate { get; set; }

        public DateTime? ExpectedHarvestDate { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "Growing";
    }
}