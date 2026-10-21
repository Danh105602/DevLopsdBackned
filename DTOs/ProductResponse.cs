namespace WebApplication1.DTOs
{
    public class ProductResponse
    {
        public int Id { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public int WarrantyPeriod { get; set; }
    }
}