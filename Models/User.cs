using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        [MaxLength(20)]
        public string Role { get; set; } = "Farmer";

        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Một User có thể quản lý nhiều Farm
        public ICollection<Farm> Farms { get; set; } = new List<Farm>();
    }
}