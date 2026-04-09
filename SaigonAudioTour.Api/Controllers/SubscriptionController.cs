using SaigonAudioTour.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace SaigonAudioTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly SubscriptionStore _store;

    public SubscriptionController(SubscriptionStore store)
    {
        _store = store;
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

        return Ok(new PremiumStatus
        {
            UserId = userId,
            IsPremium = false,
            PlanId = "free",
            PremiumUntil = null
        });
    }

    [HttpPost("create-order")]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var plan = _store.Plans.FirstOrDefault(p => p.Id == request.PlanId);
        if (plan is null)
        {
            return NotFound(new { message = "Không tìm thấy gói dịch vụ." });
        }

        var orderId = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var qrPayload = $"lgbtour://pay?orderId={orderId}&amount={plan.Price:0}&plan={plan.Id}";
        var qrImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=320x320&data={Uri.EscapeDataString(qrPayload)}";

        var order = new PaymentOrder
        {
            OrderId = orderId,
            UserId = request.UserId,
            PlanId = plan.Id,
            Amount = plan.Price,
            Currency = plan.Currency,
            Status = "pending",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            QrImageUrl = qrImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        _store.Orders.Add(order);

        return Ok(order);
    }

    [HttpGet("order-status/{orderId}")]
    public IActionResult GetOrderStatus(string orderId)
    {
        var order = _store.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order is null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        if (order.Status == "pending" && DateTime.UtcNow > order.ExpiresAt)
        {
            order.Status = "expired";
        }

        return Ok(order);
    }

    // Endpoint demo: sau khi quét QR xong có thể gọi endpoint này để đổi trạng thái paid
    [HttpPost("mark-paid/{orderId}")]
    public IActionResult MarkPaid(string orderId)
    {
        var order = _store.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order is null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        if (order.Status == "expired")
        {
            return BadRequest(new { message = "Đơn hàng đã hết hạn." });
        }

        order.Status = "paid";

        var plan = _store.Plans.First(p => p.Id == order.PlanId);
        _store.UserPremium[order.UserId] = new PremiumStatus
        {
            UserId = order.UserId,
            IsPremium = true,
            PlanId = plan.Id,
            PremiumUntil = DateTime.UtcNow.AddDays(plan.DurationDays)
        };

        return Ok(new { message = "Kích hoạt Premium thành công.", orderId = order.OrderId });
    }
}

public class CreateOrderRequest
{
    public string UserId { get; set; } = "demo-user";
    public string PlanId { get; set; } = "premium_month";
}
