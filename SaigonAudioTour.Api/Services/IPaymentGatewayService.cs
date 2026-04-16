namespace SaigonAudioTour.Api.Services;

/// <summary>
/// Contract for payment gateway implementations (VNPay, MoMo, Stripe, etc.)
/// Each provider implements this interface to handle payment processing uniformly.
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Create a payment request with the gateway.
    /// Returns gateway-specific payment URL or transaction details.
    /// </summary>
    Task<GatewayPaymentResponse> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query payment status from gateway.
    /// </summary>
    Task<PaymentStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm/capture payment after user authorization.
    /// Only needed for two-phase payment gateways (authorize + capture).
    /// </summary>
    Task<GatewayPaymentResponse> ConfirmPaymentAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund a completed payment.
    /// </summary>
    Task<RefundResponse> RefundAsync(string transactionId, decimal amount, string reason = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate incoming webhook signature to prevent tampering.
    /// Returns true if signature is valid.
    /// </summary>
    Task<bool> ValidateWebhookAsync(string signature, string payloadJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get provider name for logging/debugging (e.g., "VNPay", "MoMo").
    /// </summary>
    string GetProviderName();
}

/// <summary>
/// Request model for creating a payment.
/// </summary>
public class PaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Description { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Gateway response after payment creation.
/// </summary>
public class GatewayPaymentResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// URL for user to complete payment (for redirect-based gateways).
    /// </summary>
    public string? PaymentUrl { get; set; }
    
    /// <summary>
    /// QR code data/URL for QR-based payments.
    /// </summary>
    public string? QrCode { get; set; }
    
    /// <summary>
    /// Gateway-specific response payload.
    /// </summary>
    public Dictionary<string, object>? RawResponse { get; set; }
}

/// <summary>
/// Payment status enum.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3,
    Refunded = 4,
    Unknown = 5
}

/// <summary>
/// Refund response from gateway.
/// </summary>
public class RefundResponse
{
    public bool Success { get; set; }
    public string RefundId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal RefundedAmount { get; set; }
    public Dictionary<string, object>? RawResponse { get; set; }
}
