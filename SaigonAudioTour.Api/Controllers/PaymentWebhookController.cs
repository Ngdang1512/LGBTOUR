using Microsoft.AspNetCore.Mvc;
using SaigonAudioTour.Api.Services;

namespace SaigonAudioTour.Api.Controllers;

/// <summary>
/// Webhook endpoint for payment gateway callbacks.
/// Handles confirmation callbacks from VNPay, MoMo, and other payment providers.
/// </summary>
[ApiController]
[Route("api/payment/webhook")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentGatewayOrchestrator _paymentOrchestrator;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(IPaymentGatewayOrchestrator paymentOrchestrator, ILogger<PaymentWebhookController> logger)
    {
        _paymentOrchestrator = paymentOrchestrator ?? throw new ArgumentNullException(nameof(paymentOrchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// VNPay IPN (Instant Payment Notification) webhook endpoint.
    /// Called by VNPay servers to confirm payment status after user transaction.
    /// Reference: https://sandbox.vnpayment.vn/apis/docs/huong-dan-tich-hop/#quy-trinh-trich-doan-ma
    /// </summary>
    [HttpGet("vnpay")]
    public async Task<IActionResult> VNPayWebhook()
    {
        try
        {
            // VNPay sends data as query string parameters
            var queryString = Request.QueryString.ToString();
            if (string.IsNullOrWhiteSpace(queryString))
            {
                _logger.LogWarning("VNPay webhook received with no query parameters");
                return BadRequest(new { RspCode = "99", Message = "Không tìm thấy dữ liệu." });
            }

            // Extract secure hash from query
            string? secureHash = null;
            var dict = new Dictionary<string, string>();

            foreach (var param in queryString.TrimStart('?').Split('&'))
            {
                var parts = param.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0];
                    var value = Uri.UnescapeDataString(parts[1]);

                    if (key == "vnp_SecureHash")
                        secureHash = value;
                    else
                        dict[key] = value;
                }
            }

            if (string.IsNullOrWhiteSpace(secureHash))
            {
                _logger.LogWarning("VNPay webhook missing secure hash");
                return BadRequest(new { RspCode = "99", Message = "Không tìm thấy mã xác thực." });
            }

            // Reconstruct payload for signature validation
            var sortedParams = dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(sortedParams);
            payloadJson = payloadJson.Replace("\\", ""); // Unescape JSON

            // Validate webhook
            var isValid = await _paymentOrchestrator.HandleWebhookAsync("VNPay", secureHash, payloadJson);
            if (!isValid)
            {
                _logger.LogWarning("VNPay webhook validation failed");
                return BadRequest(new { RspCode = "97", Message = "Không hợp lệ." });
            }

            // Webhook processed successfully
            _logger.LogInformation("VNPay webhook processed successfully");
            return Ok(new { RspCode = "00", Message = "Xác nhận thành công." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay webhook");
            return StatusCode(500, new { RspCode = "99", Message = "Lỗi xử lý." });
        }
    }

    /// <summary>
    /// MoMo webhook endpoint for future implementation.
    /// </summary>
    [HttpPost("momo")]
    public async Task<IActionResult> MoMoWebhook([FromBody] object payload)
    {
        _logger.LogInformation("MoMo webhook received but not yet implemented");
        return Accepted(); // Return 202 to acknowledge
    }

    /// <summary>
    /// Health check endpoint for webhook infrastructure.
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }
}
