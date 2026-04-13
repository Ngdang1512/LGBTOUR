using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(120)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string FullName { get; set; } = string.Empty;
    }
}
