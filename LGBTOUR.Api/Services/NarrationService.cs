using LGBTOUR.Api.Data;
using LGBTOUR.Api.DTOs.Narrations;
using LGBTOUR.Api.Entities;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public class NarrationService : INarrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment; // Dùng để lấy đường dẫn thư mục lưu file

        public NarrationService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<NarrationDto?> AddNarrationAsync(CreateNarrationDto dto)
        {
            // 1. Kiểm tra xem POI (Quán ăn) có tồn tại không
            var poi = await _context.POIs.FindAsync(dto.PoiId);
            if (poi == null) return null;

            string? finalAudioUrl = null;

            // 2. Xử lý lưu File nếu Admin có đính kèm file
            if (dto.AudioFile != null && dto.AudioFile.Length > 0)
            {
                // Tạo tên file độc nhất để không bị trùng (Vd: e8f...3a-audio.mp3)
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.AudioFile.FileName);

                // Trỏ tới thư mục wwwroot/audios
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "audios");

                // Nếu thư mục chưa có thì tạo mới
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Copy file từ Request vào thư mục của Server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AudioFile.CopyToAsync(fileStream);
                }

                // Đường dẫn trả về cho Mobile App phát nhạc
                finalAudioUrl = $"/audios/{fileName}";
            }

            // 3. Lưu thông tin vào Database
            var narration = new Narration
            {
                POI_Id = dto.PoiId,
                LanguageCode = dto.LanguageCode,
                ContentText = dto.ContentText,
                VoiceType = dto.VoiceType,
                AudioUrl = finalAudioUrl,
                // Tạm thời set DurationSeconds = 0, hoặc bạn có thể dùng thư viện để đọc độ dài file mp3 sau
                DurationSeconds = 0
            };

            _context.Narrations.Add(narration);
            await _context.SaveChangesAsync();

            // 4. Trả về DTO
            return new NarrationDto
            {
                Id = narration.Id,
                LanguageCode = narration.LanguageCode,
                ContentText = narration.ContentText,
                AudioUrl = narration.AudioUrl,
                VoiceType = narration.VoiceType
            };
        }
    }
}