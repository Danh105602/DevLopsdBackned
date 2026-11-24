
namespace WebApplication1.DTOs.Farm
{
    public class FarmResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public double Area { get; set; }

        public int OwnerId { get; set; }

        public DateTime CreatedAt { get; set; }

        public FarmOwnerDto? Owner { get; set; }

        public List<FarmFieldDto> Fields { get; set; } = new();
    }

    public class FarmOwnerDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }

    public class FarmFieldDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int FarmId { get; set; }

        public string SoilType { get; set; } = string.Empty;

        public double Area { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
