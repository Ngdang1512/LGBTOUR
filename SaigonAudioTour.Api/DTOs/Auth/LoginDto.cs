using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}