using SaigonAudioTour.Api.Models.Realtime;

namespace SaigonAudioTour.Api.Services;

public interface IUserActivityStore
{
    int OnlineCount { get; }
    void Upsert(ActivityTelemetryDto telemetry);
    IReadOnlyCollection<ActivityTelemetryDto> GetSnapshot();
    void RemoveByConnection(string connectionId);
    void LinkConnection(string connectionId, string deviceId);
    int RemoveInactive(TimeSpan staleAfter);
}
