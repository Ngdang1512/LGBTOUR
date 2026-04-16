using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Services;

/// <summary>
/// Service that orchestrates payment gateway operations with logging, idempotency, and error handling.
/// This layer sits between controllers and specific gateway implementations (VNPay, MoMo, etc.).
/// </summary>
public interface IPaymentGatewayOrchestrator
{
    Task<PaymentOrderResponse> CreatePaymentAsync(
        string orderId,
        string userId,
        string planId,
        decimal amount,
        string returnUrl,
        string webhookUrl,
        CancellationToken cancellationToken = default);

    Task<(PaymentStatus Status, string Message)> GetPaymentStatusAsync(
        string orderId,
        CancellationToken cancellationToken = default);

    Task<bool> HandleWebhookAsync(
        string gatewayName,
        string signature,
        string payloadJson,
        CancellationToken cancellationToken = default);
}

public class PaymentGatewayOrchestrator : IPaymentGatewayOrchestrator
{
    private readonly IPaymentGatewayService _gatewayService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PaymentGatewayOrchestrator> _logger;

    public PaymentGatewayOrchestrator(
        IPaymentGatewayService gatewayService,
        ApplicationDbContext dbContext,
        ILogger<PaymentGatewayOrchestrator> logger)
    {
        _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentOrderResponse> CreatePaymentAsync(
        string orderId,
        string userId,
        string planId,
        decimal amount,
        string returnUrl,
        string webhookUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentNullException(nameof(orderId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        try
        {
            // Check for duplicate payment (idempotency)
            var existingTransaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.OrderId == orderId && t.UserId == userId, cancellationToken);

            if (existingTransaction != null && existingTransaction.Status == PaymentTransactionStatus.Pending)
            {
                _logger.LogInformation("Duplicate payment request for order {OrderId}, returning existing transaction", orderId);
                return new PaymentOrderResponse
                {
                    OrderId = orderId,
                    TransactionId = existingTransaction.TransactionId,
                    PaymentUrl = existingTransaction.PaymentUrl,
                    Amount = amount,
                    Success = true,
                    Message = "Yêu cầu thanh toán đã tồn tại"
                };
            }

            // Create payment via gateway
            var paymentRequest = new PaymentRequest
            {
                OrderId = orderId,
                UserId = userId,
                PlanId = planId,
                Amount = amount,
                Currency = "VND",
                Description = $"Nâng cấp gói {planId}",
                ReturnUrl = returnUrl,
                WebhookUrl = webhookUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "planId", planId }
                }
            };

            var gatewayResponse = await _gatewayService.CreatePaymentAsync(paymentRequest, cancellationToken);

            if (!gatewayResponse.Success)
            {
                _logger.LogWarning("Gateway payment creation failed for order {OrderId}: {Message}", orderId, gatewayResponse.Message);
                return new PaymentOrderResponse
                {
                    Success = false,
                    Message = gatewayResponse.Message
                };
            }

            // Log transaction
            var transaction = new PaymentTransaction
            {
                OrderId = orderId,
                UserId = userId,
                PlanId = planId,
                Amount = amount,
                GatewayName = _gatewayService.GetProviderName(),
                TransactionId = gatewayResponse.TransactionId,
                PaymentUrl = gatewayResponse.PaymentUrl,
                Status = PaymentTransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.PaymentTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment created for order {OrderId}, transaction {TransactionId}", orderId, gatewayResponse.TransactionId);

            return new PaymentOrderResponse
            {
                OrderId = orderId,
                TransactionId = gatewayResponse.TransactionId,
                PaymentUrl = gatewayResponse.PaymentUrl,
                QrCode = gatewayResponse.QrCode,
                Amount = amount,
                Success = true,
                Message = "Yêu cầu thanh toán tạo thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for order {OrderId}", orderId);
            return new PaymentOrderResponse
            {
                Success = false,
                Message = $"Lỗi tạo yêu cầu thanh toán: {ex.Message}"
            };
        }
    }

    public async Task<(PaymentStatus Status, string Message)> GetPaymentStatusAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentNullException(nameof(orderId));

        try
        {
            var transaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.OrderId == orderId, cancellationToken);

            if (transaction == null)
            {
                return (PaymentStatus.Unknown, "Không tìm thấy yêu cầu thanh toán");
            }

            // Query gateway for latest status
            var gatewayStatus = await _gatewayService.GetPaymentStatusAsync(transaction.TransactionId, cancellationToken);

            // Update local record
            transaction.Status = MapPaymentStatus(gatewayStatus);
            transaction.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (gatewayStatus, GetStatusMessage(gatewayStatus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for order {OrderId}", orderId);
            return (PaymentStatus.Unknown, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<bool> HandleWebhookAsync(
        string gatewayName,
        string signature,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(payloadJson))
            return false;

        try
        {
            // Validate webhook signature
            var isValid = await _gatewayService.ValidateWebhookAsync(signature, payloadJson, cancellationToken);
            if (!isValid)
            {
                _logger.LogWarning("Invalid webhook signature from {GatewayName}", gatewayName);
                return false;
            }

            // Parse webhook payload
            using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            // Extract order ID and status from different gateway formats
            string? orderId = null;
            PaymentStatus? status = null;

            if (gatewayName == "VNPay")
            {
                if (root.TryGetProperty("vnp_TxnRef", out var txnRef))
                    orderId = txnRef.GetString();
                if (root.TryGetProperty("vnp_ResponseCode", out var respCode) && respCode.TryGetInt32(out var code))
                    status = code == 00 ? PaymentStatus.Completed : code == 02 ? PaymentStatus.Failed : PaymentStatus.Pending;
            }

            if (string.IsNullOrWhiteSpace(orderId) || status == null)
            {
                _logger.LogWarning("Could not extract order ID or status from {GatewayName} webhook", gatewayName);
                return false;
            }

            // Update transaction
            var transaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.OrderId == orderId, cancellationToken);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for order {OrderId}", orderId);
                return true; // Still return true to acknowledge webhook
            }

            transaction.Status = MapPaymentStatus(status.Value);
            transaction.UpdatedAt = DateTime.UtcNow;

            if (status.Value == PaymentStatus.Completed)
            {
                // Mark order as paid and grant subscription
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == transaction.UserId, cancellationToken);
                if (user != null)
                {
                    user.SubscriptionStatus = "premium";
                    _logger.LogInformation("Subscription activated for user {UserId} via order {OrderId}", transaction.UserId, orderId);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Webhook processed for order {OrderId}, status {Status}", orderId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook from {GatewayName}", gatewayName);
            return false;
        }
    }

    private PaymentTransactionStatus MapPaymentStatus(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => PaymentTransactionStatus.Pending,
        PaymentStatus.Completed => PaymentTransactionStatus.Completed,
        PaymentStatus.Failed => PaymentTransactionStatus.Failed,
        PaymentStatus.Cancelled => PaymentTransactionStatus.Cancelled,
        PaymentStatus.Refunded => PaymentTransactionStatus.Refunded,
        _ => PaymentTransactionStatus.Unknown
    };

    private string GetStatusMessage(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Chờ thanh toán",
        PaymentStatus.Completed => "Thanh toán thành công",
        PaymentStatus.Failed => "Thanh toán thất bại",
        PaymentStatus.Cancelled => "Thanh toán bị hủy",
        PaymentStatus.Refunded => "Đã hoàn tiền",
        _ => "Trạng thái không xác định"
    };
}

/// <summary>
/// Response model for payment creation - sent to mobile app.
/// Compatible with existing UpgradePage expectations.
/// </summary>
public class PaymentOrderResponse
{
    public string OrderId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PaymentUrl { get; set; }
    public string? QrCode { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
