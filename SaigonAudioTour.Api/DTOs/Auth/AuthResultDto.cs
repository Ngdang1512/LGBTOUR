namespace SaigonAudioTour.Api.DTOs.Auth
{
    public class AuthResultDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string SubscriptionStatus { get; set; } = "free";
    }
}
