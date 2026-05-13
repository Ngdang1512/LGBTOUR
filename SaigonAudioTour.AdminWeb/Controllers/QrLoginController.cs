using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaigonAudioTour.AdminWeb.Models;
using System.Net.Http.Json;

namespace SaigonAudioTour.AdminWeb.Controllers;

[AllowAnonymous]
public class QrLoginController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public QrLoginController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("/demo-login")]
    [HttpGet("/qr-login")]
    public async Task<IActionResult> Index([FromQuery] string? u, [FromQuery] string? p, [FromQuery] string? plan)
    {
        var vm = new QrLoginViewModel
        {
            InputUsername = (u ?? string.Empty).Trim(),
            InputPlanHint = (plan ?? string.Empty).Trim(),
            HasInput = !string.IsNullOrWhiteSpace(u) && !string.IsNullOrWhiteSpace(p)
        };

        if (!vm.HasInput)
        {
            vm.IsSuccess = false;
            vm.Message = "Thiếu dữ liệu đăng nhập từ QR. Cần có u (email) và p (password).";
            return View(vm);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var loginRes = await client.PostAsJsonAsync("api/auth/login", new
            {
                username = vm.InputUsername,
                password = p
            });

            if (!loginRes.IsSuccessStatusCode)
            {
                vm.IsSuccess = false;
                vm.Message = "Đăng nhập thất bại từ QR (sai tài khoản/mật khẩu hoặc API chưa sẵn sàng).";
                return View(vm);
            }

            var loginJson = await loginRes.Content.ReadFromJsonAsync<JsonElement>();
            vm.IsSuccess = true;
            vm.Message = "Đăng nhập thành công bằng QR (web demo).";

            vm.UserId = loginJson.TryGetProperty("userId", out var uidEl) && uidEl.TryGetInt32(out var uid) ? uid : 0;
            vm.Email = loginJson.TryGetProperty("email", out var emailEl) ? (emailEl.GetString() ?? vm.InputUsername) : vm.InputUsername;
            vm.FullName = loginJson.TryGetProperty("fullName", out var fullNameEl) ? (fullNameEl.GetString() ?? string.Empty) : string.Empty;

            var token = loginJson.TryGetProperty("token", out var tokenEl) ? (tokenEl.GetString() ?? string.Empty) : string.Empty;
            vm.TokenPreview = string.IsNullOrWhiteSpace(token)
                ? string.Empty
                : (token.Length > 28 ? token[..28] + "..." : token);

            var profileRes = await client.GetAsync($"api/auth/profile?email={Uri.EscapeDataString(vm.Email)}");
            if (profileRes.IsSuccessStatusCode)
            {
                var profileJson = await profileRes.Content.ReadFromJsonAsync<JsonElement>();
                vm.SubscriptionStatus = profileJson.TryGetProperty("subscriptionStatus", out var subEl)
                    ? (subEl.GetString() ?? "unknown")
                    : "unknown";
            }

            if (!string.IsNullOrWhiteSpace(vm.InputPlanHint))
            {
                vm.PlanMatchesHint = string.Equals(vm.InputPlanHint, vm.SubscriptionStatus, StringComparison.OrdinalIgnoreCase)
                                    || (string.Equals(vm.InputPlanHint, "FREE", StringComparison.OrdinalIgnoreCase)
                                        && string.Equals(vm.SubscriptionStatus, "free", StringComparison.OrdinalIgnoreCase));
            }
        }
        catch (Exception ex)
        {
            vm.IsSuccess = false;
            vm.Message = $"Lỗi kết nối API: {ex.Message}";
        }

        return View(vm);
    }
}
