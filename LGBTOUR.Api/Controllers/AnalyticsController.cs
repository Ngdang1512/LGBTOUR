using Microsoft.AspNetCore.Mvc;
using LGBTOUR.Api.Data;
using LGBTOUR.Api.Entities;
using LGBTOUR.Api.DTOs;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Analytics/log-audio
        [HttpPost("log-audio")]
        public async Task<IActionResult> LogAudioListen([FromBody] UserLogDto request)
        {
            try
            {
                // 1. Chuyển đổi dữ liệu từ DTO sang Entity thực tế của Database
                var log = new UserLog
                {
                    UserId = string.IsNullOrEmpty(request.UserId) ? "Anonymous" : request.UserId,
                    POI_Id = request.POI_Id,
                    POIId = request.POI_Id, // Gán cho cả 2 trường để khớp với model của bạn
                    Lat = request.Lat,
                    Lng = request.Lng,
                    DurationSeconds = request.DurationSeconds,
                    EventType = "Nghe Audio",
                    CreatedAt = DateTime.Now
                };

                // 2. Lưu vào Database
                _context.UserLogs.Add(log);
                await _context.SaveChangesAsync();

                // 3. Trả về thông báo thành công cho Mobile App biết
                return Ok(new { success = true, message = "Lưu Analytics thành công!" });
            }
            catch (Exception ex)
            {
                // Nếu lỗi, trả về mã 500 kèm thông báo
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}