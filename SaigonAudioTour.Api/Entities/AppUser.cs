using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.Entities
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? AvatarUrl { get; set; }

        [Required]
        [MaxLength(30)]
        public string SubscriptionStatus { get; set; } = "free";

        /// <summary>Ngày hết hạn gói Premium. Null = chưa mua hoặc gói free.</summary>
        public DateTime? PremiumExpiresAt { get; set; }
    }
}
