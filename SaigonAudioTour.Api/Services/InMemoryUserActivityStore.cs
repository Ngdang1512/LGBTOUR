using System.Collections.Concurrent;
using SaigonAudioTour.Api.Models.Realtime;

namespace SaigonAudioTour.Api.Services;

public sealed class InMemoryUserActivityStore : IUserActivityStore
{
    private readonly ConcurrentDictionary<string, ActivityTelemetryDto> _activityByDevice = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _deviceByConnection = new(StringComparer.OrdinalIgnoreCase);

    public int OnlineCount => _activityByDevice.Count;

    public void Upsert(ActivityTelemetryDto telemetry)
    {
        if (telemetry == null || string.IsNullOrWhiteSpace(telemetry.DeviceId))
        {
            return;
        }

        telemetry.Timestamp = telemetry.Timestamp == default ? DateTimeOffset.UtcNow : telemetry.Timestamp;
        _activityByDevice[telemetry.DeviceId] = telemetry;
    }

    public IReadOnlyCollection<ActivityTelemetryDto> GetSnapshot()
    {
        return _activityByDevice.Values
            .OrderByDescending(x => x.Timestamp)
            .ToList();
    }

    public void LinkConnection(string connectionId, string deviceId)
    {
        if (string.IsNullOrWhiteSpace(connectionId) || string.IsNullOrWhiteSpace(deviceId))
        {
            return;
        }

        _deviceByConnection[connectionId] = deviceId;
    }

    public void RemoveByConnection(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            return;
        }

        if (_deviceByConnection.TryRemove(connectionId, out var deviceId))
        {
            _activityByDevice.TryRemove(deviceId, out _);
        }
    }

    public int RemoveInactive(TimeSpan staleAfter)
    {
        if (staleAfter <= TimeSpan.Zero)
        {
            return 0;
        }

        var threshold = DateTimeOffset.UtcNow.Subtract(staleAfter);
        var removed = 0;

        foreach (var item in _activityByDevice.ToArray())
        {
            if (item.Value.Timestamp < threshold)
            {
                if (_activityByDevice.TryRemove(item.Key, out _))
                {
                    removed++;
                }
            }
        }

        return removed;
    }
}
