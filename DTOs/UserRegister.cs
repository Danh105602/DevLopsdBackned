using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class UserRegister
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username không được để trống")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Username phải từ 3 đến 50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100,
            ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password không được để trống")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Password phải từ 6 đến 100 ký tự")]
        public string Password { get; set; } = string.Empty;
    }
}