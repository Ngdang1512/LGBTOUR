using LGBTOUR.AdminWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LGBTOUR.AdminWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = User.FindFirst("JWToken")?.Value;

            // Gắn thẻ bài cho các API cần bảo mật
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 1. Gọi API lấy Top 5 POI được nghe nhiều nhất
            var topPois = new List<PoiStatisticViewModel>();
            var topPoisResponse = await client.GetAsync("api/Dashboard/top-pois?top=5");
            if (topPoisResponse.IsSuccessStatusCode)
            {
                topPois = await topPoisResponse.Content.ReadFromJsonAsync<List<PoiStatisticViewModel>>() ?? new List<PoiStatisticViewModel>();
            }

            // 2. Gọi API lấy Toàn bộ POI (để lấy tọa độ Lat/Lng vẽ lên bản đồ)
            var allPois = new List<PoiViewModel>();
            var poisResponse = await client.GetAsync("api/Pois");
            if (poisResponse.IsSuccessStatusCode)
            {
                allPois = await poisResponse.Content.ReadFromJsonAsync<List<PoiViewModel>>() ?? new List<PoiViewModel>();
            }

            // 3. Ráp dữ liệu Tọa độ + Số lượt nghe để vẽ Heatmap
            var heatMapData = topPois.Select(stat => {
                var poi = allPois.FirstOrDefault(p => p.Id == stat.PoiId);
                return new
                {
                    lat = poi?.Lat ?? 10.7769,
                    lng = poi?.Lng ?? 106.7009,
                    // Nghe càng nhiều thì cường độ (intensity) càng đậm
                    intensity = stat.TotalListens * 0.3
                };
            }).ToList();

            // Đóng gói thành JSON quăng xuống View
            ViewBag.TopPOIsData = JsonSerializer.Serialize(topPois);
            ViewBag.HeatMapData = JsonSerializer.Serialize(heatMapData);

            return View();
        }
    }
}