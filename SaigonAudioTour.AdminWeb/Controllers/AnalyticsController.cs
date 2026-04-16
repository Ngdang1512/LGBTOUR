using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

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
                var apiUrl = _configuration["ApiUrl"] ?? "http://localhost:5000";
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/heatmap?startDate=2026-04-01&endDate=2026-04-16&groupBy=poi");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Không thể tải dữ liệu analytics";
                    return View("Dashboard", new HeatmapData());
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var heatmapData = JsonSerializer.Deserialize<HeatmapData>(content, options);

                return View("Dashboard", heatmapData ?? new HeatmapData());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
                return View("Dashboard", new HeatmapData());
            }
        }

        public async Task<IActionResult> TopPois()
        {
            try
            {
                var apiUrl = _configuration["ApiUrl"] ?? "http://localhost:5000";
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/heatmap?startDate=2026-04-01&endDate=2026-04-16&groupBy=poi");

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
                var apiUrl = _configuration["ApiUrl"] ?? "http://localhost:5000";
                var response = await _httpClient.GetAsync($"{apiUrl}/api/dashboard/heatmap?startDate=2026-04-01&endDate=2026-04-16&groupBy=poi");

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
}
