using SaigonAudioTour.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Chức năng: App Mobile sẽ liên tục gửi tọa độ của người dùng lên đây để kiểm tra
        [HttpGet("check-nearby")]
        public async Task<IActionResult> CheckNearby(double userLat, double userLng)
        {
            var allPOIs = await _context.POIs.ToListAsync();
            var nearbyPOIs = new List<object>();

            foreach (var poi in allPOIs)
            {
                // Gọi hàm toán học tính khoảng cách
                double distance = CalculateDistance(userLat, userLng, poi.Lat, poi.Lng);

                // Nếu khoảng cách tính được NHỎ HƠN bán kính (Radius) của địa điểm -> Đã bước vào vùng!
                if (distance <= poi.Radius)
                {
                    nearbyPOIs.Add(new
                    {
                        poiName = poi.Name,
                        distance = Math.Round(distance, 1) + " mét",
                        message = $"Bạn đã bước vào vùng của {poi.Name}. Lệnh cho App: Tự động phát Audio!"
                    });
                }
            }

            // Nếu có địa điểm nào ở gần, trả về danh sách đó
            if (nearbyPOIs.Any())
            {
                return Ok(new { success = true, activePOIs = nearbyPOIs });
            }

            // Nếu đang đứng bơ vơ giữa đường
            return Ok(new { success = false, message = "Bạn chưa đến gần địa điểm tham quan nào." });
        }

        // HÀM TOÁN HỌC HAVERSINE (Tính khoảng cách giữa 2 tọa độ GPS)
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // Bán kính Trái Đất tính bằng mét
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Trả về khoảng cách bằng mét
        }
    }
}