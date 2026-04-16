using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace SaigonAudioTour.Mobile.Services
{
    /// <summary>
    /// DTO cho UserLog API
    /// </summary>
    public class UserLogDto
    {
        public int UserId { get; set; }
        public int PoiId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DurationSeconds { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeviceId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service để record user narration playback events
    /// Dùng cho admin heatmap analytics
    /// </summary>
    public class UserLogService
    {
        private readonly HttpClient _httpClient;
        private const string DeviceIdKey = "DeviceId";

        public UserLogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Log narration playback event
        /// POST /api/userlogs/checkin
        /// </summary>
        public async Task<bool> LogNarrationPlaybackAsync(int userId, int poiId, double latitude, double longitude, double durationSeconds)
        {
            try
            {
                var deviceId = GetOrCreateDeviceId();
                var log = new UserLogDto
                {
                    UserId = userId,
                    PoiId = poiId,
                    Latitude = latitude,
                    Longitude = longitude,
                    DurationSeconds = durationSeconds,
                    Timestamp = DateTime.UtcNow,
                    DeviceId = deviceId
                };

                var response = await _httpClient.PostAsJsonAsync("api/userlogs/checkin", log);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserLogService] Error logging playback: {ex.Message}");
                return false;
            }
        }

        private static string GetOrCreateDeviceId()
        {
            var existing = Preferences.Get(DeviceIdKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(existing))
            {
                return existing;
            }

            var deviceId = Guid.NewGuid().ToString("N");
            Preferences.Set(DeviceIdKey, deviceId);
            return deviceId;
        }
    }
}
