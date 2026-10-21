using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Username không được để trống")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Username phải từ 3 đến 50 ký tự")]
        public string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password không được để trống")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Password phải từ 6 đến 100 ký tự")]
        public string Password { get; set; } = string.Empty;
    }
}