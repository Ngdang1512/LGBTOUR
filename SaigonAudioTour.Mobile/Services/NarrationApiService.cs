using System.Net.Http.Json;

namespace SaigonAudioTour.Mobile.Services;

public class NarrationApiService
{
    private readonly HttpClient _httpClient;

    public NarrationApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<NarrationContent?> GetNarrationByPoiAsync(int poiId, string languageCode)
    {
        try
        {
            var lang = string.IsNullOrWhiteSpace(languageCode) ? "vi" : languageCode.Trim().ToLowerInvariant();
            var dto = await _httpClient.GetFromJsonAsync<NarrationApiDto>($"api/narrations/{poiId}?lang={lang}");
            if (dto == null)
            {
                return null;
            }

            var audioUrl = dto.AudioUrl ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(audioUrl) && audioUrl.StartsWith("/"))
            {
                audioUrl = $"{_httpClient.BaseAddress?.ToString().TrimEnd('/')}{audioUrl}";
            }

            return new NarrationContent
            {
                PoiId = poiId,
                LanguageCode = string.IsNullOrWhiteSpace(dto.LanguageCode) ? lang : dto.LanguageCode,
                ContentText = dto.ContentText ?? string.Empty,
                AudioUrl = audioUrl
            };
        }
        catch
        {
            return null;
        }
    }

    private sealed class NarrationApiDto
    {
        public string? LanguageCode { get; set; }
        public string? ContentText { get; set; }
        public string? AudioUrl { get; set; }
    }

    public sealed class NarrationContent
    {
        public int PoiId { get; set; }
        public string LanguageCode { get; set; } = "vi";
        public string ContentText { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
    }
}
