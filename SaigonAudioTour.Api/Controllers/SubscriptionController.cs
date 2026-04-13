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
            PlanId = "default",
            PremiumUntil = null
        });
    }

    [HttpPost("create-order")]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
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

        var orderId = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var qrPayload = $"saigonaudiotour://pay?orderId={orderId}&amount={plan.Price:0}&plan={plan.Id}";
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
