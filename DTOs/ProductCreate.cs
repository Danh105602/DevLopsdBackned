using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class ProductCreate
    {
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Mã sản phẩm phải từ 2 đến 50 ký tự")]
        public string ProductCode { get; set; } = string.Empty;


        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Tên sản phẩm phải từ 2 đến 100 ký tự")]
        public string ProductName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Danh mục không được để trống")]
        [StringLength(50,
            ErrorMessage = "Danh mục không được vượt quá 50 ký tự")]
        public string Category { get; set; } = string.Empty;


        [Required(ErrorMessage = "Thương hiệu không được để trống")]
        [StringLength(50,
            ErrorMessage = "Thương hiệu không được vượt quá 50 ký tự")]
        public string Brand { get; set; } = string.Empty;


        [Required(ErrorMessage = "Model không được để trống")]
        [StringLength(100,
            ErrorMessage = "Model không được vượt quá 100 ký tự")]
        public string Model { get; set; } = string.Empty;


        [Required(ErrorMessage = "Serial Number không được để trống")]
        [StringLength(100,
            ErrorMessage = "Serial Number không được vượt quá 100 ký tự")]
        public string SerialNumber { get; set; } = string.Empty;


        [Required(ErrorMessage = "Ngày mua không được để trống")]
        public DateTime PurchaseDate { get; set; }


        [Range(1, 120,
            ErrorMessage = "Thời hạn bảo hành phải từ 1 đến 120 tháng")]
        public int WarrantyPeriod { get; set; }
    }
}