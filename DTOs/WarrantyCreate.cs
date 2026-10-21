using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class WarrantyCreate
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}