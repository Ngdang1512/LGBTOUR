using LGBTOUR.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Chức năng: App gọi API này để xin file âm thanh và đoạn chữ thuyết minh
        [HttpGet("get-media")]
        public async Task<IActionResult> GetMedia(int poiId, string langCode = "vi")
        {
            // 1. Tìm đoạn văn bản (Narration) và file âm thanh (Audio) ở 2 bảng khác nhau
            var narration = await _context.Narrations.FirstOrDefaultAsync(n => n.POI_Id == poiId && n.LanguageCode == langCode);
            var audio = await _context.Audios.FirstOrDefaultAsync(a => a.POI_Id == poiId && a.LanguageCode == langCode);

            // 2. Cơ chế Fallback (Dự phòng): Khách chọn tiếng lạ chưa có thì tự động bật tiếng Việt
            if (narration == null && audio == null && langCode != "vi")
            {
                narration = await _context.Narrations.FirstOrDefaultAsync(n => n.POI_Id == poiId && n.LanguageCode == "vi");
                audio = await _context.Audios.FirstOrDefaultAsync(a => a.POI_Id == poiId && a.LanguageCode == "vi");
            }

            // Nếu tiếng Việt cũng không có nốt (Admin chưa nhập dữ liệu)
            if (narration == null && audio == null)
            {
                return NotFound(new { message = "Chưa có nội dung thuyết minh cho địa điểm này." });
            }

            // 3. Đóng gói 2 mảnh ghép lại thành 1 cục gửi cho App
            return Ok(new
            {
                poiId = poiId,
                language = narration?.LanguageCode ?? audio?.LanguageCode,
                textContent = narration?.Content ?? "",
                audioUrl = audio?.AudioUrl ?? "",
                duration = audio?.Duration ?? 0
            });
        }
    }
}