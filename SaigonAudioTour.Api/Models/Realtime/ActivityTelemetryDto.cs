namespace SaigonAudioTour.Api.Models.Realtime;

public sealed class ActivityTelemetryDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Status { get; set; } = "moving";
    public string? ActionType { get; set; }  // NEW: Loại hành động (detail, navigation, listening, idle, moving)
    public int? PoiId { get; set; }
    public string? PoiName { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
