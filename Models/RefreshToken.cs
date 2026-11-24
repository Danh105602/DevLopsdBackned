
namespace WebApplication1.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // Chỉ lưu giá trị hash, không lưu token gốc
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        // Null nghĩa là token chưa bị thu hồi
        public DateTime? RevokedAt { get; set; }

        // Hash của token mới sau khi rotate
        public string? ReplacedByTokenHash { get; set; }
    }
}
