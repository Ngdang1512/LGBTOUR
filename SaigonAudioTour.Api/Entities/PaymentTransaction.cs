namespace SaigonAudioTour.Api.Entities;

/// <summary>
/// Tracks all payment attempts with idempotency support.
/// Ensures transactions are not duplicated even if requests are retried.
/// </summary>
public class PaymentTransaction
{
    public int Id { get; set; }
    
    /// <summary>
    /// Order ID from subscription system - used for idempotency key.
    /// </summary>
    public string OrderId { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID who initiated payment.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Subscription plan ID (e.g., "premium").
    /// </summary>
    public string PlanId { get; set; } = string.Empty;
    
    /// <summary>
    /// Payment amount in smallest currency unit (e.g., 99000 VND).
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment gateway name (VNPay, MoMo, Stripe, etc.).
    /// </summary>
    public string GatewayName { get; set; } = "VNPay";
    
    /// <summary>
    /// Transaction ID from payment gateway - used to query status.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Payment URL for redirect-based gateways.
    /// </summary>
    public string? PaymentUrl { get; set; }
    
    /// <summary>
    /// Current transaction status.
    /// </summary>
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;
    
    /// <summary>
    /// Optional error message if payment failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Optional refund reason if transaction was refunded.
    /// </summary>
    public string? RefundReason { get; set; }
    
    /// <summary>
    /// Timestamp when transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when transaction was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when payment was confirmed (if applicable).
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }
}

/// <summary>
/// Payment transaction status enum.
/// </summary>
public enum PaymentTransactionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3,
    Refunded = 4,
    Unknown = 5
}
