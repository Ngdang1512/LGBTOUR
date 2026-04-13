using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.Entities
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Bắt buộc phải lưu mật khẩu đã mã hóa, không lưu mật khẩu thô

        public string? FullName { get; set; }
    }
}