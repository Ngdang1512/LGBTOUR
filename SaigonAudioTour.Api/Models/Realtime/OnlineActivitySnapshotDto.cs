namespace SaigonAudioTour.Api.Models.Realtime;

public sealed class OnlineActivitySnapshotDto
{
    public int OnlineCount { get; set; }
    public List<ActivityTelemetryDto> Users { get; set; } = new();
}
