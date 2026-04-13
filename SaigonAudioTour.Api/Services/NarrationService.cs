using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.DTOs.Narrations;
using SaigonAudioTour.Api.Entities;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public class NarrationService : INarrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ITtsService _ttsService;

        public NarrationService(ApplicationDbContext context, IWebHostEnvironment environment, ITtsService ttsService)
        {
            _context = context;
            _environment = environment;
            _ttsService = ttsService;
        }

        public async Task<NarrationDto?> AddNarrationAsync(CreateNarrationDto dto)
        {
            var poi = await _context.POIs.FindAsync(dto.PoiId);
            if (poi == null) return null;

            string? finalAudioUrl = null;

            // If audio file is provided, use it
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
            // If no audio file, generate from text using TTS
            else if (!string.IsNullOrWhiteSpace(dto.ContentText))
            {
                finalAudioUrl = await _ttsService.TextToSpeechAsync(dto.ContentText, dto.LanguageCode);
            }

            // 1. Create Narration record (text content)
            var narration = new Narration
            {
                POI_Id = dto.PoiId,
                LanguageCode = dto.LanguageCode,
                ContentText = dto.ContentText,
                TranslatedName = poi.Name,
                AudioUrl = finalAudioUrl,
                DurationSeconds = 0
            };

            _context.Narrations.Add(narration);
            await _context.SaveChangesAsync();

            // 2. Create Audio record (file path) - linked to same POI
            if (!string.IsNullOrWhiteSpace(finalAudioUrl))
            {
                var audio = new Audio
                {
                    POI_Id = dto.PoiId,
                    LanguageCode = dto.LanguageCode,
                    AudioUrl = finalAudioUrl,
                    Duration = 0 // TODO: Calculate from file
                };

                _context.Audios.Add(audio);
                await _context.SaveChangesAsync();
            }

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