using SaigonAudioTour.Api.Services;
using Microsoft.AspNetCore.Mvc;
using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly SubscriptionStore _store;
    private readonly ApplicationDbContext _context;
    private readonly IPaymentGatewayOrchestrator _paymentOrchestrator;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        SubscriptionStore store,
        ApplicationDbContext context,
        IPaymentGatewayOrchestrator paymentOrchestrator,
        ILogger<SubscriptionController> logger)
    {
        _store = store;
        _context = context;
        _paymentOrchestrator = paymentOrchestrator ?? throw new ArgumentNullException(nameof(paymentOrchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("plans")]
    public IActionResult GetPlans()
    {
        return Ok(_store.Plans);
    }

    [HttpGet("user/{userId}/status")]
    public IActionResult GetUserStatus(string userId)
    {
        if (_store.UserPremium.TryGetValue(userId, out var status))
        {
            return Ok(status);
        }

        if (int.TryParse(userId, out var parsedUserId))
        {
            var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == parsedUserId);
            if (user != null)
            {
                var isPremium = string.Equals(user.SubscriptionStatus, "premium", StringComparison.OrdinalIgnoreCase);
                return Ok(new PremiumStatus
                {
                    UserId = userId,
                    IsPremium = isPremium,
                    PlanId = isPremium ? "premium" : "default",
                    Status = string.IsNullOrWhiteSpace(user.SubscriptionStatus) ? "free" : user.SubscriptionStatus,
                    PremiumUntil = isPremium ? DateTime.UtcNow.AddDays(30) : null
                });
            }
        }

        return Ok(new PremiumStatus
        {
            UserId = userId,
            IsPremium = false,
            PlanId = "default",
            Status = "free",
            PremiumUntil = null
        });
    }

    /// <summary>
    /// Create payment order using production gateway (VNPay).
    /// Returns payment URL for user to complete transaction.
    /// Supports idempotency - duplicate requests return cached response.
    /// </summary>
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new { message = "Thiếu userId." });
        }

        var plan = _store.Plans.FirstOrDefault(p => p.Id == request.PlanId);
        if (plan is null)
        {
            return NotFound(new { message = "Không tìm thấy gói dịch vụ." });
        }

        if (plan.Id == "default")
        {
            return BadRequest(new { message = "Gói mặc định không cần thanh toán." });
        }

        if (IsUserPremium(request.UserId))
        {
            return BadRequest(new { message = "Tài khoản của bạn đã là Premium." });
        }

        try
        {
            var orderId = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            
            // Use production payment gateway (VNPay)
            var paymentResponse = await _paymentOrchestrator.CreatePaymentAsync(
                orderId,
                request.UserId,
                plan.Id,
                plan.Price,
                "http://localhost:8000/payment/return", // Return URL
                "http://localhost:5000/api/payment/webhook/vnpay" // Webhook URL
            );

            if (!paymentResponse.Success)
            {
                _logger.LogWarning("Payment creation failed: {Message}", paymentResponse.Message);
                return BadRequest(new { message = paymentResponse.Message });
            }

            // For backward compatibility with mobile app, return PaymentOrder format
            var order = new PaymentOrder
            {
                OrderId = paymentResponse.OrderId,
                UserId = request.UserId,
                PlanId = plan.Id,
                Amount = plan.Price,
                Currency = plan.Currency,
                Status = "pending",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                QrImageUrl = paymentResponse.QrCode ?? paymentResponse.PaymentUrl ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", request.UserId);
            return StatusCode(500, new { message = "Lỗi tạo yêu cầu thanh toán." });
        }
    }

    [HttpGet("order-status/{orderId}")]
    public async Task<IActionResult> GetOrderStatus(string orderId)
    {
        try
        {
            var (status, message) = await _paymentOrchestrator.GetPaymentStatusAsync(orderId);

            var order = _context.PaymentTransactions
                .FirstOrDefault(t => t.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            return Ok(new
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                Status = order.Status.ToString(),
                Amount = order.Amount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Message = message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order status for {OrderId}", orderId);
            return StatusCode(500, new { message = "Lỗi truy vấn trạng thái." });
        }
    }

    /// <summary>
    /// Manual payment confirmation - called when webhook fails.
    /// Should only be used for testing/fallback scenarios.
    /// </summary>
    [HttpPost("mark-paid/{orderId}")]
    public IActionResult MarkPaid(string orderId)
    {
        var transaction = _context.PaymentTransactions
            .FirstOrDefault(t => t.OrderId == orderId);

        if (transaction == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        if (transaction.Status != PaymentTransactionStatus.Pending)
        {
            return BadRequest(new { message = $"Trạng thái đơn hàng không phù hợp: {transaction.Status}" });
        }

        // Mark as completed
        transaction.Status = PaymentTransactionStatus.Completed;
        transaction.ConfirmedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

        // Activate subscription
        if (int.TryParse(transaction.UserId, out var userId))
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.SubscriptionStatus = "premium";
            }
        }

        _context.SaveChanges();

        return Ok(new { message = "Kích hoạt Premium thành công.", orderId = transaction.OrderId });
    }

    [HttpPost("cancel/{userId}")]
    public IActionResult CancelSubscription(string userId)
    {
        _store.UserPremium[userId] = new PremiumStatus
        {
            UserId = userId,
            IsPremium = false,
            PlanId = "default",
            Status = "cancelled",
            PremiumUntil = null
        };

        UpdateUserSubscriptionStatus(userId, "cancelled");

        return Ok(new { message = "Đã huỷ gói đăng ký." });
    }

    private bool IsUserPremium(string userId)
    {
        if (_store.UserPremium.TryGetValue(userId, out var premiumStatus))
        {
            return premiumStatus.IsPremium;
        }

        if (!int.TryParse(userId, out var parsedUserId))
        {
            return false;
        }

        return _context.Users.AsNoTracking().Any(user => user.Id == parsedUserId && user.SubscriptionStatus == "premium");
    }

    private void UpdateUserSubscriptionStatus(string userId, string subscriptionStatus)
    {
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return;
        }

        var user = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
        if (user == null)
        {
            return;
        }

        user.SubscriptionStatus = subscriptionStatus;
        _context.SaveChanges();
    }
}
