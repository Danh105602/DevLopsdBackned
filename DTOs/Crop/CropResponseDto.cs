
namespace WebApplication1.DTOs.Crop
{
    public class CropResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int GrowthDays { get; set; }
    }
}
