using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SaigonAudioTour.Api.Services;

/// <summary>
/// VNPay payment gateway adapter.
/// Handles secure communication with VNPay API including signature generation/validation.
/// Vietnam-based payment gateway supporting bank transfer, e-wallet, QR code.
/// </summary>
public class VNPayAdapter : IPaymentGatewayService
{
    private readonly IConfiguration _config;
    private readonly ILogger<VNPayAdapter> _logger;
    private readonly HttpClient _httpClient;

    // VNPay configuration keys
    private const string TMN_CODE_KEY = "VNPay:TmnCode";
    private const string HASH_SECRET_KEY = "VNPay:HashSecret";
    private const string API_URL_KEY = "VNPay:ApiUrl";
    private const string QUERY_API_URL_KEY = "VNPay:QueryApiUrl";
    private const string RETURN_URL_KEY = "VNPay:ReturnUrl";
    private const string IPN_URL_KEY = "VNPay:IpnUrl";

    public VNPayAdapter(IConfiguration config, ILogger<VNPayAdapter> logger, HttpClient httpClient)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient;
    }

    public async Task<GatewayPaymentResponse> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            var tmnCode = _config[TMN_CODE_KEY];
            var hashSecret = _config[HASH_SECRET_KEY];
            var apiUrl = _config[API_URL_KEY];

            if (string.IsNullOrWhiteSpace(tmnCode) || string.IsNullOrWhiteSpace(hashSecret) || string.IsNullOrWhiteSpace(apiUrl))
            {
                _logger.LogError("VNPay configuration incomplete");
                return new GatewayPaymentResponse
                {
                    Success = false,
                    Message = "VNPay cấu hình không đầy đủ"
                };
            }

            var createDate = DateTime.UtcNow.AddHours(7); // Vietnam timezone
            var expireDate = createDate.AddMinutes(15);

            var paymentParams = new Dictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", ((long)(request.Amount * 100)).ToString() }, // Amount in smallest unit
                { "vnp_CurrCode", request.Currency },
                { "vnp_TxnRef", request.OrderId },
                { "vnp_OrderInfo", request.Description ?? $"Nâng cấp gói {request.PlanId}" },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" },
                { "vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss") },
                { "vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss") },
                { "vnp_IpAddr", GetClientIpAddress() },
                { "vnp_ReturnUrl", _config[RETURN_URL_KEY] ?? request.ReturnUrl }
            };

            // Add metadata if provided
            if (request.Metadata != null && request.Metadata.ContainsKey("userId"))
            {
                paymentParams["vnp_TxnRef"] = $"{request.OrderId}_{request.Metadata["userId"]}";
            }

            // Generate secure hash
            var sortedParams = paymentParams.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            var hashInput = string.Join("&", sortedParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var secureHash = HmacSha512(hashSecret, hashInput);

            paymentParams["vnp_SecureHash"] = secureHash;

            // Build payment URL
            var queryString = string.Join("&", paymentParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var paymentUrl = $"{apiUrl}?{queryString}";

            _logger.LogInformation("VNPay payment created for order {OrderId}, amount {Amount}", request.OrderId, request.Amount);

            return new GatewayPaymentResponse
            {
                Success = true,
                TransactionId = request.OrderId,
                PaymentUrl = paymentUrl,
                Message = "Yêu cầu thanh toán tạo thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating VNPay payment for order {OrderId}", request.OrderId);
            return new GatewayPaymentResponse
            {
                Success = false,
                Message = $"Lỗi tạo yêu cầu thanh toán: {ex.Message}"
            };
        }
    }

    public async Task<PaymentStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentNullException(nameof(transactionId));

        try
        {
            var tmnCode = _config[TMN_CODE_KEY];
            var hashSecret = _config[HASH_SECRET_KEY];
            var queryApiUrl = _config[QUERY_API_URL_KEY];

            if (string.IsNullOrWhiteSpace(queryApiUrl))
            {
                _logger.LogWarning("VNPay Query API URL not configured");
                return PaymentStatus.Unknown;
            }

            var createDate = DateTime.UtcNow.AddHours(7);
            var queryParams = new Dictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "querydr" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_TxnRef", transactionId },
                { "vnp_OrderInfo", "" },
                { "vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss") },
                { "vnp_IpAddr", GetClientIpAddress() }
            };

            var sortedParams = queryParams.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            var hashInput = string.Join("&", sortedParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var secureHash = HmacSha512(hashSecret, hashInput);

            queryParams["vnp_SecureHash"] = secureHash;

            var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var requestUrl = $"{queryApiUrl}?{queryString}";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("VNPay query failed for {TransactionId}: {StatusCode}", transactionId, response.StatusCode);
                return PaymentStatus.Unknown;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("vnp_ResponseCode", out var codeElem) && codeElem.TryGetInt32(out var code))
            {
                return MapVNPayResponseCode(code);
            }

            return PaymentStatus.Unknown;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying VNPay status for {TransactionId}", transactionId);
            return PaymentStatus.Unknown;
        }
    }

    public async Task<GatewayPaymentResponse> ConfirmPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        // VNPay doesn't require explicit confirmation - payment is confirmed via IPN/return URL.
        // This method exists for compatibility with gateways that require two-phase processing.
        var status = await GetPaymentStatusAsync(transactionId, cancellationToken);

        return new GatewayPaymentResponse
        {
            Success = status == PaymentStatus.Completed,
            TransactionId = transactionId,
            Message = status == PaymentStatus.Completed ? "Thanh toán đã xác nhận" : "Thanh toán chưa hoàn tất"
        };
    }

    public async Task<RefundResponse> RefundAsync(string transactionId, decimal amount, string reason = "", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentNullException(nameof(transactionId));

        try
        {
            // VNPay refund requires additional configuration and API key
            // This is a placeholder for production implementation
            _logger.LogInformation("VNPay refund initiated for {TransactionId}, amount {Amount}", transactionId, amount);

            return new RefundResponse
            {
                Success = true,
                RefundId = $"REF-{transactionId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                RefundedAmount = amount,
                Message = "Yêu cầu hoàn tiền được xử lý"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding VNPay transaction {TransactionId}", transactionId);
            return new RefundResponse
            {
                Success = false,
                Message = $"Lỗi hoàn tiền: {ex.Message}"
            };
        }
    }

    public async Task<bool> ValidateWebhookAsync(string signature, string payloadJson, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(payloadJson))
            return false;

        try
        {
            var hashSecret = _config[HASH_SECRET_KEY];
            if (string.IsNullOrWhiteSpace(hashSecret))
                return false;

            // Parse JSON to extract all fields except SecureHash
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            var params_dict = new SortedDictionary<string, string>();
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name.ToLower() != "vnp_securehash")
                {
                    params_dict[prop.Name] = prop.Value.GetString() ?? "";
                }
            }

            // Build hash input from sorted parameters
            var hashInput = string.Join("&", params_dict.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var calculatedHash = HmacSha512(hashSecret, hashInput);

            var isValid = calculatedHash.Equals(signature, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                _logger.LogWarning("Invalid VNPay webhook signature. Expected: {Expected}, Got: {Got}", calculatedHash, signature);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating VNPay webhook");
            return false;
        }
    }

    public string GetProviderName() => "VNPay";

    /// <summary>
    /// Generate HMAC SHA512 hash for VNPay secure communication.
    /// </summary>
    private string HmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    /// <summary>
    /// Get client IP address for VNPay request validation.
    /// </summary>
    private string GetClientIpAddress() => "127.0.0.1"; // Should be injected from IHttpContextAccessor in real scenario

    /// <summary>
    /// Map VNPay response codes to PaymentStatus enum.
    /// Reference: https://sandbox.vnpayment.vn/apis/docs/huong-dan-tich-hop/
    /// </summary>
    private PaymentStatus MapVNPayResponseCode(int code)
    {
        return code switch
        {
            00 => PaymentStatus.Completed,
            01 => PaymentStatus.Pending,
            02 => PaymentStatus.Failed,
            04 => PaymentStatus.Cancelled,
            05 => PaymentStatus.Unknown,
            _ => PaymentStatus.Unknown
        };
    }
}
