using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.DTOs.Narrations;
using SaigonAudioTour.Api.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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

        private bool IsMockTtsService => _ttsService.GetType().Name.Contains("MockTtsService", StringComparison.OrdinalIgnoreCase);

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
                DurationSeconds = 0,
                VoiceType = string.IsNullOrWhiteSpace(finalAudioUrl) ? "Client TTS" : "Cloud TTS"
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

        public async Task<NarrationDto?> GetNarrationByPoiAndLanguageAsync(int poiId, string? languageCode)
        {
            var normalizedLang = string.IsNullOrWhiteSpace(languageCode)
                ? "vi"
                : languageCode.Trim().ToLowerInvariant();

            var narration = await _context.Narrations
                .AsNoTracking()
                .Where(n => n.POI_Id == poiId)
                .OrderBy(n => n.Id)
                .FirstOrDefaultAsync(n => n.LanguageCode.ToLower() == normalizedLang);

            // Nếu user yêu cầu ngôn ngữ khác và DB chưa có bản đó,
            // trả text đúng ngôn ngữ yêu cầu (localized fallback), KHÔNG trả text tiếng Việt.
            if (narration == null && normalizedLang != "vi")
            {
                var poi = await _context.POIs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == poiId);

                if (poi == null)
                {
                    return null;
                }

                return new NarrationDto
                {
                    Id = 0,
                    LanguageCode = normalizedLang,
                    ContentText = BuildLocalizedNarrationText(poi, normalizedLang),
                    AudioUrl = null,
                    DurationSeconds = 0,
                    VoiceType = "Client TTS Localized",
                    TranslatedName = poi.Name
                };
            }

            narration ??= await _context.Narrations
                .AsNoTracking()
                .Where(n => n.POI_Id == poiId)
                .OrderBy(n => n.Id)
                .FirstOrDefaultAsync(n => n.LanguageCode.ToLower() == "vi");

            narration ??= await _context.Narrations
                .AsNoTracking()
                .Where(n => n.POI_Id == poiId)
                .OrderBy(n => n.Id)
                .FirstOrDefaultAsync();

            if (narration == null)
            {
                return null;
            }

            var audioUrl = narration.AudioUrl;
            if (string.IsNullOrWhiteSpace(audioUrl))
            {
                var audio = await _context.Audios
                    .AsNoTracking()
                    .Where(a => a.POI_Id == poiId)
                    .OrderBy(a => a.Id)
                    .FirstOrDefaultAsync(a => a.LanguageCode.ToLower() == narration.LanguageCode.ToLower());

                // Với tiếng Việt: cho phép fallback sang bất kỳ audio nào của POI đó
                // (audio có thể được upload mà không gắn languageCode cụ thể).
                // Với ngôn ngữ khác: KHÔNG fallback — tránh phát nhầm audio tiếng Việt khi chọn English.
                if (audio == null && narration.LanguageCode.ToLower() == "vi")
                {
                    audio = await _context.Audios
                        .AsNoTracking()
                        .Where(a => a.POI_Id == poiId)
                        .OrderBy(a => a.Id)
                        .FirstOrDefaultAsync();
                }

                audioUrl = audio?.AudioUrl;
            }

            return new NarrationDto
            {
                Id = narration.Id,
                LanguageCode = narration.LanguageCode,
                ContentText = narration.ContentText ?? string.Empty,
                AudioUrl = audioUrl,
                DurationSeconds = narration.DurationSeconds,
                VoiceType = narration.VoiceType,
                TranslatedName = narration.TranslatedName ?? string.Empty
            };
        }

        public async Task<NarrationDto?> GenerateTtsFallbackAsync(int poiId, string? languageCode)
        {
            var poi = await _context.POIs.FindAsync(poiId);
            if (poi == null) return null;

            var lang = string.IsNullOrWhiteSpace(languageCode)
                ? "vi"
                : languageCode.Trim().ToLowerInvariant();
            var text = BuildLocalizedNarrationText(poi, lang);

            // Development guard: mock TTS creates test tones (beep), so keep audioUrl null
            // and let clients use local Web Speech TTS instead.
            string? audioUrl = null;
            if (!IsMockTtsService)
            {
                audioUrl = await _ttsService.TextToSpeechAsync(text, lang);
            }

            var narration = await _context.Narrations
                .FirstOrDefaultAsync(n => n.POI_Id == poiId && n.LanguageCode.ToLower() == lang);

            if (narration == null)
            {
                narration = new Narration
                {
                    POI_Id = poiId,
                    LanguageCode = lang,
                    TranslatedName = poi.Name,
                    ContentText = text,
                    AudioUrl = audioUrl,
                    DurationSeconds = 0,
                    VoiceType = string.IsNullOrWhiteSpace(audioUrl) ? "Client TTS" : "Cloud TTS"
                };

                _context.Narrations.Add(narration);
            }
            else
            {
                narration.ContentText = text;
                narration.TranslatedName = string.IsNullOrWhiteSpace(narration.TranslatedName) ? poi.Name : narration.TranslatedName;
                narration.AudioUrl = audioUrl;
                narration.VoiceType = string.IsNullOrWhiteSpace(audioUrl) ? "Client TTS" : "Cloud TTS";
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(audioUrl))
            {
                var audio = await _context.Audios
                    .FirstOrDefaultAsync(a => a.POI_Id == poiId && a.LanguageCode.ToLower() == lang);

                if (audio == null)
                {
                    _context.Audios.Add(new Audio
                    {
                        POI_Id = poiId,
                        LanguageCode = lang,
                        AudioUrl = audioUrl,
                        Duration = 0
                    });
                }
                else
                {
                    audio.AudioUrl = audioUrl;
                }

                await _context.SaveChangesAsync();
            }

            return new NarrationDto
            {
                Id = narration.Id,
                LanguageCode = narration.LanguageCode,
                ContentText = narration.ContentText,
                AudioUrl = narration.AudioUrl,
                DurationSeconds = narration.DurationSeconds,
                VoiceType = narration.VoiceType,
                TranslatedName = narration.TranslatedName
            };
        }

        private static string BuildLocalizedNarrationText(POI poi, string languageCode)
        {
            var name = string.IsNullOrWhiteSpace(poi.Name) ? "POI" : poi.Name;
            var detail = string.IsNullOrWhiteSpace(poi.Description) ? name : poi.Description;
            var lang = string.IsNullOrWhiteSpace(languageCode) ? "vi" : languageCode.Trim().ToLowerInvariant();

            return lang switch
            {
                "en" => $"Welcome to {name}. This is one of the notable destinations in Ho Chi Minh City. Please look around and enjoy the local culture.",
                "zh" => $"欢迎来到{name}。这里是胡志明市著名景点之一，请慢慢参观并感受当地文化。",
                "ja" => $"{name}へようこそ。ここはホーチミン市を代表する観光スポットの一つです。周辺の雰囲気と文化をお楽しみください。",
                "ko" => $"{name}에 오신 것을 환영합니다. 이곳은 호치민시의 대표적인 관광 명소 중 하나입니다. 주변 분위기와 문화를 천천히 즐겨보세요.",
                "fr" => $"Bienvenue à {name}. C'est l'un des sites emblématiques de Hô Chi Minh-Ville. Prenez le temps de découvrir l'ambiance et la culture locale.",
                _ => detail
            };
        }
    }
}
