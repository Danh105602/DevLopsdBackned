namespace WebApplication1.DTOs
{
    public class WarrantyResponse
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}