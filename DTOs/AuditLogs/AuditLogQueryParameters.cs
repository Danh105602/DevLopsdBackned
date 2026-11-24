namespace WebApplication1.DTOs.AuditLogs
{
    public class AuditLogQueryParameters
    {
        private const int MaxPageSize = 100;

        private int _pageNumber = 1;
        private int _pageSize = 20;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 20 : Math.Min(value, MaxPageSize);
        }

        // Bộ lọc (tất cả đều tùy chọn)
        public int? UserId { get; set; }
        public string? Action { get; set; }        // CREATE | UPDATE | DELETE
        public string? EntityName { get; set; }    // Farm | Field | Crop | Planting
        public int? EntityId { get; set; }

        // Ngày theo UTC, truyền dạng yyyy-MM-dd. "To" gồm cả ngày đó.
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}