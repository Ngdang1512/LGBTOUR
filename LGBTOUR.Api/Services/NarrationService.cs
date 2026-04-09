using LGBTOUR.Api.Data;
using LGBTOUR.Api.DTOs.Narrations;
using LGBTOUR.Api.Entities;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public class NarrationService : INarrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public NarrationService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<NarrationDto?> AddNarrationAsync(CreateNarrationDto dto)
        {
            var poi = await _context.POIs.FindAsync(dto.PoiId);
            if (poi == null) return null;

            string? finalAudioUrl = null;

            if (dto.AudioFile != null && dto.AudioFile.Length > 0)
            {
                // Bảo mật: Chỉ cho phép file âm thanh
                var extension = Path.GetExtension(dto.AudioFile.FileName).ToLower();
                var allowedExtensions = new[] { ".mp3", ".wav", ".m4a" };

                if (allowedExtensions.Contains(extension))
                {
                    var fileName = Guid.NewGuid().ToString() + extension;
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "audios");

                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.AudioFile.CopyToAsync(fileStream);
                    }
                    finalAudioUrl = $"/audios/{fileName}";
                }
            }

            var narration = new Narration
            {
                POI_Id = dto.PoiId,
                LanguageCode = dto.LanguageCode,
                ContentText = dto.ContentText,
                TranslatedName = poi.Name, // Lấy tạm tên gốc nếu chưa dịch
                VoiceType = dto.VoiceType,
                AudioUrl = finalAudioUrl,
                DurationSeconds = 0
            };

            _context.Narrations.Add(narration);
            await _context.SaveChangesAsync();

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