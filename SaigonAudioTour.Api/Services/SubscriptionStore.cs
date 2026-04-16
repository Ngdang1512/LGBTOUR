namespace SaigonAudioTour.Api.Services;

public class SubscriptionStore
{
    public List<PremiumPlan> Plans { get; } = new()
    {
        new PremiumPlan("default", "Gói mặc định", 0, "VND", 0, "Truy cập cơ bản"),
        new PremiumPlan("premium", "Gói Premium", 99000, "VND", 30, "Mở toàn bộ audio + không quảng cáo + ưu tiên trải nghiệm")
    };

    public List<PaymentOrder> Orders { get; } = new();
    public Dictionary<string, PremiumStatus> UserPremium { get; } = new();
}

public record PremiumPlan(
    string Id,
    string Name,
    decimal Price,
    string Currency,
    int DurationDays,
    string Features
);

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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PremiumStatus
{
    public string UserId { get; set; } = string.Empty;
    public bool IsPremium { get; set; }
    public string PlanId { get; set; } = "default";
    public string Status { get; set; } = "free";
    public DateTime? PremiumUntil { get; set; }
}

public class CreateOrderRequest
{
    public string UserId { get; set; } = string.Empty;
    public string PlanId { get; set; } = "premium";
}
