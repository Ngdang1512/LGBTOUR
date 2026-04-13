namespace SaigonAudioTour.Mobile.Models;

public class PremiumPlan
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public int DurationDays { get; set; }
    public string Features { get; set; } = string.Empty;
}

public class PaymentOrder
{
    public string OrderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = "pending";
    public DateTime ExpiresAt { get; set; }
    public string QrImageUrl { get; set; } = string.Empty;
}

public class PremiumStatus
{
    public string UserId { get; set; } = "demo-user";
    public bool IsPremium { get; set; }
    public string PlanId { get; set; } = "free";
    public DateTime? PremiumUntil { get; set; }
}
