using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System;

namespace SaigonAudioTour.AdminWeb.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AnalyticsController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var apiUrl = GetApiUrl();
                var (startDate, endDate) = GetDefaultDateRange(30);
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/heatmap?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&groupBy=poi");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Không thể tải dữ liệu analytics";
                    return View("Dashboard", new HeatmapData());
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var heatmapData = JsonSerializer.Deserialize<HeatmapData>(content, options);

                var revenueResponse = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/revenue-summary?days=7");
                if (revenueResponse.IsSuccessStatusCode)
                {
                    var revenueContent = await revenueResponse.Content.ReadAsStringAsync();
                    var revenueSummary = JsonSerializer.Deserialize<RevenueSummary>(revenueContent, options);
                    ViewBag.RevenueSummary = revenueSummary ?? new RevenueSummary();
                }
                else
                {
                    ViewBag.RevenueSummary = new RevenueSummary();
                }

                return View("Dashboard", heatmapData ?? new HeatmapData());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
                ViewBag.RevenueSummary = new RevenueSummary();
                return View("Dashboard", new HeatmapData());
            }
        }

        public async Task<IActionResult> TopPois()
        {
            try
            {
                var apiUrl = GetApiUrl();
                var (startDate, endDate) = GetDefaultDateRange(30);
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/heatmap?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&groupBy=poi");

                if (!response.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Lỗi tải dữ liệu" });

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var heatmapData = JsonSerializer.Deserialize<HeatmapData>(content, options);

                var topPois = heatmapData?.Items?
                    .OrderByDescending(x => x.VisitCount)
                    .Take(10)
                    .Select(x => new { x.PoiName, x.VisitCount, x.AvgDuration })
                    .ToList();

                return Json(new { success = true, data = topPois });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> ChartData()
        {
            try
            {
                var apiUrl = GetApiUrl();
                var (startDate, endDate) = GetDefaultDateRange(30);
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/heatmap?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&groupBy=poi");

                if (!response.IsSuccessStatusCode)
                    return Json(new { labels = new string[] { }, datasets = new object[] { } });

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var heatmapData = JsonSerializer.Deserialize<HeatmapData>(content, options);

                var labels = heatmapData?.Items?.Select(x => x.PoiName).ToArray() ?? new string[] { };
                var visitCounts = heatmapData?.Items?.Select(x => x.VisitCount).ToArray() ?? new int[] { };
                var avgDurations = heatmapData?.Items?.Select(x => x.AvgDuration).ToArray() ?? new int[] { };

                return Json(new
                {
                    labels = labels,
                    datasets = new[]
                    {
                        new { label = "Lượt nghe", data = visitCounts, borderColor = "rgb(75, 192, 192)", backgroundColor = "rgba(75, 192, 192, 0.1)" },
                        new { label = "Thời gian TB (giây)", data = avgDurations, borderColor = "rgb(255, 99, 132)", backgroundColor = "rgba(255, 99, 132, 0.1)" }
                    }
                });
            }
            catch
            {
                return Json(new { labels = new string[] { }, datasets = new object[] { } });
            }
        }

        public async Task<IActionResult> RevenueSummary(int days = 7)
        {
            try
            {
                var apiUrl = GetApiUrl();
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/revenue-summary?days={days}");
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Lỗi tải dữ liệu doanh thu" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<RevenueSummary>(content, options) ?? new RevenueSummary();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> RevenueTrend(int days = 7)
        {
            try
            {
                var apiUrl = GetApiUrl();
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/revenue-summary?days={days}");
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { labels = new string[] { }, datasets = new object[] { } });
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<RevenueSummary>(content, options) ?? new RevenueSummary();

                var labels = data.Trend.Select(x => x.Date.Length >= 10 ? x.Date.Substring(5) : x.Date).ToArray();
                var revenues = data.Trend.Select(x => x.Revenue).ToArray();
                var premiumPurchases = data.Trend.Select(x => x.PremiumPurchases).ToArray();

                return Json(new
                {
                    labels,
                    datasets = new object[]
                    {
                        new { label = "Doanh thu (VND)", data = revenues, borderColor = "rgb(34, 197, 94)", backgroundColor = "rgba(34, 197, 94, 0.25)", yAxisID = "yRevenue" },
                        new { label = "Mua Premium", data = premiumPurchases, borderColor = "rgb(59, 130, 246)", backgroundColor = "rgba(59, 130, 246, 0.20)", yAxisID = "yCount" }
                    }
                });
            }
            catch
            {
                return Json(new { labels = new string[] { }, datasets = new object[] { } });
            }
        }

        public async Task<IActionResult> ActiveUsers(int minutes = 5)
        {
            try
            {
                var apiUrl = GetApiUrl();
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/active-users?minutes={minutes}");
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Lỗi tải dữ liệu người dùng đang hoạt động" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<ActiveUsersSnapshot>(content, options) ?? new ActiveUsersSnapshot();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string GetApiUrl() => _configuration["ApiUrl"] ?? "http://localhost:5117";

        private static (DateTime StartDate, DateTime EndDate) GetDefaultDateRange(int days)
        {
            var end = DateTime.UtcNow.Date;
            var start = end.AddDays(-(Math.Max(days, 1) - 1));
            return (start, end);
        }
    }

    public class HeatmapData
    {
        [System.Text.Json.Serialization.JsonPropertyName("heatmapData")]
        public List<HeatmapItem> Items { get; set; } = new();
    }

    public class HeatmapItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("poiId")]
        public int PoiId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("poiName")]
        public string? PoiName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("visitCount")]
        public int VisitCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("avgDuration")]
        public int AvgDuration { get; set; }
    }

    public class RevenueSummary
    {
        [System.Text.Json.Serialization.JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("totalRevenue")]
        public decimal TotalRevenue { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ticketsSold")]
        public int TicketsSold { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("completedPayments")]
        public int CompletedPayments { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("activePremiumUsers")]
        public int ActivePremiumUsers { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("totalUsers")]
        public int TotalUsers { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("premiumBuyers")]
        public int PremiumBuyers { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("conversionRate")]
        public decimal ConversionRate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("arpu")]
        public decimal Arpu { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("trend")]
        public List<RevenueTrendItem> Trend { get; set; } = new();
    }

    public class RevenueTrendItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("revenue")]
        public decimal Revenue { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("premiumPurchases")]
        public int PremiumPurchases { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("completedPayments")]
        public int CompletedPayments { get; set; }
    }

    public class ActiveUsersSnapshot
    {
        [System.Text.Json.Serialization.JsonPropertyName("windowMinutes")]
        public int WindowMinutes { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("asOfUtc")]
        public DateTime AsOfUtc { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("activeUsers")]
        public int ActiveUsers { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("activeDevices")]
        public int ActiveDevices { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("activeAccounts")]
        public int ActiveAccounts { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public List<ActiveUserItem> Items { get; set; } = new();
    }

    public class ActiveUserItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("sessionKey")]
        public string SessionKey { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("lastSeenAt")]
        public DateTime LastSeenAt { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("heartbeatCount")]
        public int HeartbeatCount { get; set; }
    }
}
