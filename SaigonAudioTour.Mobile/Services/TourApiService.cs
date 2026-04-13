// SaigonAudioTour.Mobile/Services/TourApiService.cs
using SaigonAudioTour.Mobile.Models;
using System.Net.Http.Json;

namespace SaigonAudioTour.Mobile.Services;

public class TourApiService
{
    public const string UserIdKey = "UserId";
    public const string UserEmailKey = "UserEmail";
    public const string UserFullNameKey = "UserFullName";
    public const string AuthTokenKey = "AuthToken";

    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = DeviceInfo.Platform == DevicePlatform.Android
            ? new Uri("http://10.0.2.2:5117/")
            : new Uri("http://localhost:5117/")
    };

    public TourApiService()
    {
    }

    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
            {
                username = email,
                password
            });

            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AuthResult>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<AuthResult?> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
            {
                fullName,
                email,
                password
            });

            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AuthResult>();
        }
        catch
        {
            return null;
        }
    }

    // 1. Dữ liệu giả lập cho bản đồ: tuyến xe buýt 2 tầng Quận 1
    public async Task<List<Place>> GetRoutePlacesAsync()
    {
        try
        {
            var pois = await _httpClient.GetFromJsonAsync<List<PoiApiDto>>("api/pois") ?? new List<PoiApiDto>();
            return pois
                .OrderByDescending(p => p.Priority)
                .Select(MapPlace)
                .ToList();
        }
        catch
        {
            return new List<Place>();
        }
    }

    // 2. Dữ liệu danh sách Trang chủ: đồng bộ tuyến xe buýt 2 tầng Quận 1
    public async Task<List<Place>> GetAllPlacesAsync()
    {
        return await GetRoutePlacesAsync();
    }

    // 3. Dữ liệu giả lập cho Trang cá nhân
    public async Task<UserProfile?> GetUserProfileAsync(string email)
    {
        try
        {
            var encodedEmail = Uri.EscapeDataString(email ?? string.Empty);
            var response = await _httpClient.GetAsync($"api/auth/profile?email={encodedEmail}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserProfile>();
        }
        catch
        {
            return null;
        }
    }

    // 4. Dữ liệu cho đồ án: tuyến xe buýt 2 tầng Quận 1
    public async Task<List<Place>> GetProjectPlacesAsync()
    {
        return await GetRoutePlacesAsync();
    }

    // 5. Subscription plans cho màn hình nâng cấp
    public async Task<List<PremiumPlan>> GetPremiumPlansAsync()
    {
        try
        {
            var plans = await _httpClient.GetFromJsonAsync<List<PremiumPlan>>("api/subscription/plans") ?? new List<PremiumPlan>();
            return plans.Where(p => p.Id is "default" or "premium").ToList();
        }
        catch
        {
            return new List<PremiumPlan>
            {
                new() { Id = "default", Name = "Gói mặc định", Price = 0, Currency = "VND", DurationDays = 0, Features = "Truy cập cơ bản" },
                new() { Id = "premium", Name = "Gói Premium", Price = 99000, Currency = "VND", DurationDays = 30, Features = "Mở toàn bộ audio + không quảng cáo + ưu tiên trải nghiệm" }
            };
        }
    }

    public async Task<PaymentOrder?> CreateUpgradeOrderAsync(string userId, string planId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/subscription/create-order", new { userId, planId });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PaymentOrder>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<PaymentOrder?> GetOrderStatusAsync(string orderId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PaymentOrder>($"api/subscription/order-status/{orderId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> MarkOrderPaidAsync(string orderId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/subscription/mark-paid/{orderId}", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PremiumStatus> GetPremiumStatusAsync(string userId)
    {
        try
        {
            var status = await _httpClient.GetFromJsonAsync<PremiumStatus>($"api/subscription/user/{userId}/status");
            return status ?? new PremiumStatus { UserId = userId, IsPremium = false, PlanId = "default" };
        }
        catch
        {
            return new PremiumStatus { UserId = userId, IsPremium = false, PlanId = "default" };
        }
    }

    private static Place MapPlace(PoiApiDto p)
    {
        var imageUrl = p.ImageUrl ?? p.Image ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl.StartsWith("/"))
        {
            imageUrl = $"{_httpClient.BaseAddress?.ToString().TrimEnd('/')}{imageUrl}";
        }

        return new Place
        {
            Id = p.Id,
            Name = p.Name,
            Location = !string.IsNullOrWhiteSpace(p.Location)
                ? p.Location
                : (string.IsNullOrWhiteSpace(p.Description) ? p.Name : p.Description),
            Latitude = p.Lat,
            Longitude = p.Lng,
            ImageUrl = imageUrl,
            Rating = "4.7",
            Category = p.IsStopStation ? "Trạm dừng" : "Điểm tham quan",
            TriggerRadius = p.Radius <= 0 ? 50 : p.Radius,
            Priority = p.Priority,
            TtsScript = !string.IsNullOrWhiteSpace(p.TtsScript)
                ? p.TtsScript
                : (string.IsNullOrWhiteSpace(p.Description) ? p.Name : p.Description),
            IsNarrating = false
        };
    }

    private sealed class PoiApiDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? Location { get; set; }
        public string? TtsScript { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Priority { get; set; }
        public int Radius { get; set; }
        public bool IsStopStation { get; set; }
    }

    public sealed class AuthResult
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}