using System.Net.Http.Headers;
using System.Net.Http.Json;
using AdminApp.Models;
using Microsoft.AspNetCore.Http;

namespace AdminApp.Services;

public class AdminPoiApiClient
{
    private readonly HttpClient _httpClient;

    public AdminPoiApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Poi>> GetAllAsync(string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/pois");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<Poi>();

            var payload = await response.Content.ReadFromJsonAsync<List<PoiApiDto>>() ?? new List<PoiApiDto>();
            return payload.Select(MapPoi).ToList();
        }
        catch
        {
            return new List<Poi>();
        }
    }

    public async Task<Poi?> GetByIdAsync(int id, string token)
    {
        var all = await GetAllAsync(token);
        return all.FirstOrDefault(x => x.Id == id);
    }

    public async Task<int?> CreateAsync(Poi poi, string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/pois");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new
            {
                name = poi.Name?.Trim() ?? string.Empty,
                description = poi.Description,
                lat = poi.Lat ?? 0,
                lng = poi.Lng ?? 0,
                radius = poi.Radius ?? 50,
                priority = 0,
                isStopStation = true
            });

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var created = await response.Content.ReadFromJsonAsync<PoiApiDto>();
            return created?.Id;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateAsync(Poi poi, string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/pois/{poi.Id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new
            {
                name = poi.Name?.Trim() ?? string.Empty,
                description = poi.Description,
                lat = poi.Lat ?? 0,
                lng = poi.Lng ?? 0,
                radius = poi.Radius ?? 50,
                priority = 0,
                isStopStation = true
            });

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id, string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/pois/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UploadImageAsync(int poiId, IFormFile imageFile, string token)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var stream = imageFile.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType ?? "application/octet-stream");
            content.Add(fileContent, "imageFile", imageFile.FileName);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/pois/{poiId}/image")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CreateNarrationAsync(int poiId, string languageCode, string contentText, IFormFile? audioFile, string token)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(poiId.ToString()), "PoiId");
            form.Add(new StringContent(languageCode), "LanguageCode");
            form.Add(new StringContent(contentText), "ContentText");

            if (audioFile != null && audioFile.Length > 0)
            {
                using var stream = audioFile.OpenReadStream();
                using var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(audioFile.ContentType ?? "application/octet-stream");
                form.Add(fileContent, "AudioFile", audioFile.FileName);
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/narrations")
            {
                Content = form
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static Poi MapPoi(PoiApiDto dto)
    {
        return new Poi
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Lat = dto.Lat,
            Lng = dto.Lng,
            Radius = dto.Radius,
            Image = string.IsNullOrWhiteSpace(dto.ImageUrl) ? dto.Image : dto.ImageUrl,
            AudioPath = null
        };
    }

    private sealed class PoiApiDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? ImageUrl { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Radius { get; set; }
    }
}
