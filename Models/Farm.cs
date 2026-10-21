using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Farm
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public double Area { get; set; }

        public int OwnerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation

        public User? Owner { get; set; }

        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }
}