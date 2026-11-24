namespace WebApplication1.DTOs.Plantings
{
    public class PlantingResponseDto
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public int CropId { get; set; }
        public DateTime PlantingDate { get; set; }
        public DateTime? ExpectedHarvestDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}