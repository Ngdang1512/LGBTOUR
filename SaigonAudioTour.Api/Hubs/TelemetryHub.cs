using Microsoft.AspNetCore.SignalR;
using SaigonAudioTour.Api.Services;

namespace SaigonAudioTour.Api.Hubs
{
    /// <summary>
    /// SignalR Hub for receiving and broadcasting user telemetry:
    /// - POI discovery and interaction (location, audio playback)
    /// - QR code scans
    /// - User location updates for geofencing and analytics
    /// </summary>
    public class TelemetryHub : Hub
    {
        private readonly IUserLogService _userLogService;

        public TelemetryHub(IUserLogService userLogService)
        {
            _userLogService = userLogService;
        }

        /// <summary>
        /// Client sends location update and POI event (e.g., audio playback started)
        /// Used by: WebApp (browser), Mobile (MAUI)
        /// </summary>
        public async Task LogPoiInteraction(PoiTelemetryDto data)
        {
            if (data == null) return;

            try
            {
                // Store the event in UserLog table
                await _userLogService.LogEventAsync(
                    userId: data.UserId ?? Context.ConnectionId,
                    poiId: data.PoiId,
                    eventType: data.EventType ?? "Nghe Audio",
                    lat: data.Latitude,
                    lng: data.Longitude,
                    durationSeconds: data.DurationSeconds
                );

                // Broadcast to admins for realtime dashboards
                await Clients.Group("admins").SendAsync("PoiInteractionLogged", data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TelemetryHub] LogPoiInteraction error: {ex.Message}");
            }
        }

        /// <summary>
        /// Client sends QR code scan event with scanned POI ID
        /// </summary>
        public async Task LogQrScan(QrScanTelemetryDto data)
        {
            if (data == null) return;

            try
            {
                await _userLogService.LogEventAsync(
                    userId: data.UserId ?? Context.ConnectionId,
                    poiId: data.PoiId,
                    eventType: "QR Scan",
                    lat: data.Latitude,
                    lng: data.Longitude
                );

                await Clients.Group("admins").SendAsync("QrScanLogged", data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TelemetryHub] LogQrScan error: {ex.Message}");
            }
        }

        /// <summary>
        /// Client sends location update for geofencing and proximity tracking
        /// </summary>
        public async Task SendLocationUpdate(LocationUpdateDto data)
        {
            if (data == null) return;

            try
            {
                // Log location without POI context (used for proximity analysis)
                await _userLogService.LogEventAsync(
                    userId: data.UserId ?? Context.ConnectionId,
                    poiId: null,
                    eventType: "Location Update",
                    lat: data.Latitude,
                    lng: data.Longitude
                );

                await Clients.Group("admins").SendAsync("LocationUpdated", data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TelemetryHub] SendLocationUpdate error: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin calls this to join the admin group and receive telemetry updates
        /// </summary>
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        /// <summary>
        /// Clean up on disconnect
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
            await base.OnDisconnectedAsync(exception);
        }
    }

    // DTOs
    public class PoiTelemetryDto
    {
        public string? UserId { get; set; }
        public int PoiId { get; set; }
        public string? EventType { get; set; } // "Nghe Audio", "Xem Hình Ảnh", etc.
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long? DurationSeconds { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class QrScanTelemetryDto
    {
        public string? UserId { get; set; }
        public int PoiId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class LocationUpdateDto
    {
        public string? UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
