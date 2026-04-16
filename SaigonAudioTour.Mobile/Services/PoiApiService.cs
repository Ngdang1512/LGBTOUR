using SaigonAudioTour.Mobile.Models;
using System.Net.Http.Json;

namespace SaigonAudioTour.Mobile.Services;

public class PoiApiService
{
    private readonly HttpClient _httpClient;

    public PoiApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Place>> GetPlacesAsync()
    {
        try
        {
            var pois = await _httpClient.GetFromJsonAsync<List<PoiApiDto>>("api/pois") ?? new List<PoiApiDto>();
            return pois
                .OrderByDescending(p => p.Priority)
                .Select(MapPlace)
                .ToList();
        }
        catch
        {
            return new List<Place>();
        }
    }

    private Place MapPlace(PoiApiDto p)
    {
        var imageUrl = p.ImageUrl ?? p.Image ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl.StartsWith("/"))
        {
            imageUrl = $"{_httpClient.BaseAddress?.ToString().TrimEnd('/')}{imageUrl}";
        }

        return new Place
        {
            Id = p.Id,
            Name = p.Name,
            Location = !string.IsNullOrWhiteSpace(p.Location)
                ? p.Location
                : (string.IsNullOrWhiteSpace(p.Description) ? p.Name : p.Description),
            Latitude = p.Lat,
            Longitude = p.Lng,
            ImageUrl = imageUrl,
            Rating = string.Empty,
            Category = p.IsStopStation ? "Trạm dừng" : "Điểm tham quan",
            TriggerRadius = p.Radius <= 0 ? 50 : p.Radius,
            Priority = p.Priority,
            TtsScript = !string.IsNullOrWhiteSpace(p.TtsScript)
                ? p.TtsScript
                : (string.IsNullOrWhiteSpace(p.Description) ? p.Name : p.Description),
            IsNarrating = false
        };
    }

    private sealed class PoiApiDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? Location { get; set; }
        public string? TtsScript { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Priority { get; set; }
        public int Radius { get; set; }
        public bool IsStopStation { get; set; }
    }
}
