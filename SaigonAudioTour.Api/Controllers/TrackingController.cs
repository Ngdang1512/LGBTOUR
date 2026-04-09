using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Thêm [FromQuery] để Swagger truyền dữ liệu chuẩn xác hơn
        [HttpPost("log-action")]
        public async Task<IActionResult> LogUserAction([FromQuery] int poiId, [FromQuery] string? userId, [FromQuery] string? eventType)
        {
            // 1. ÁO GIÁP CHỐNG LỖI NULL: Nếu App quên gửi ID, tự động gán tên giả để SQL khỏi báo lỗi
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = "Khach-Vang-Lai-" + Guid.NewGuid().ToString().Substring(0, 5);
            }

            if (string.IsNullOrWhiteSpace(eventType))
            {
                eventType = "Nghe Audio";
            }

            // 2. Kiểm tra xem POI có tồn tại không
            var poiExists = await _context.POIs.FindAsync(poiId);
            if (poiExists == null)
            {
                return NotFound(new { message = "Địa điểm không tồn tại." });
            }

            // 3. Đóng gói dữ liệu gửi xuống SQL
            var newLog = new UserLog
            {
                POI_Id = poiId,
                POIId = poiId,
                UserId = userId,
                EventType = eventType,
                CreatedAt = DateTime.Now,

                // --- THÊM 3 DÒNG NÀY ĐỂ CHỐNG LỖI NULL ---
                DurationSeconds = 0,       // Cho mặc định là 0 giây
                Lat = 0.0,                 // Cho tọa độ mặc định là 0
                Lng = 0.0                  // Cho tọa độ mặc định là 0
            };

            // 4. Lưu vào Database
            _context.UserLogs.Add(newLog);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã ghi nhận báo cáo thành công!",
                logId = newLog.Id,
                recordedUser = userId
            });
        }
    }
}