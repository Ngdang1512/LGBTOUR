namespace SaigonAudioTour.AdminWeb.Models;

public class QrLoginViewModel
{
    public string InputUsername { get; set; } = string.Empty;
    public string InputPlanHint { get; set; } = string.Empty;
    public bool HasInput { get; set; }

    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int UserId { get; set; }

    public string SubscriptionStatus { get; set; } = "unknown";
    public bool PlanMatchesHint { get; set; }
    public string TokenPreview { get; set; } = string.Empty;
}
