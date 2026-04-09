namespace SaigonAudioTour.Api.Services;

public class SubscriptionStore
{
    public List<PremiumPlan> Plans { get; } = new()
    {
        new PremiumPlan("premium_month", "Premium tháng", 49000, "VND", 30, "Mở toàn bộ audio + heatmap nâng cao + không quảng cáo"),
        new PremiumPlan("premium_year", "Premium năm", 299000, "VND", 365, "Toàn bộ tính năng Premium, tiết kiệm chi phí"),
        new PremiumPlan("pro_month", "Pro tháng", 99000, "VND", 30, "Premium + AI gợi ý lịch trình + thống kê cá nhân")
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
    public string PlanId { get; set; } = string.Empty;
    public DateTime? PremiumUntil { get; set; }
}
