using System.Threading.Channels;
using SaigonAudioTour.Mobile.Models;

namespace SaigonAudioTour.Mobile.Services.Geofencing;

public sealed class GeofenceConflictResolver : IAsyncDisposable
{
    private readonly Channel<QueuedPoi> _queue = Channel.CreateUnbounded<QueuedPoi>();
    private DateTimeOffset _lastTriggeredAt = DateTimeOffset.MinValue;
    private int _lastPoiId = -1;
    private CancellationTokenSource? _processorCts;

    public TimeSpan Cooldown { get; set; } = TimeSpan.FromSeconds(18);

    public event EventHandler<QueuedPoi>? OnResolved;

    public void Start()
    {
        if (_processorCts != null)
        {
            return;
        }

        _processorCts = new CancellationTokenSource();
        _ = Task.Run(() => ProcessQueueAsync(_processorCts.Token), _processorCts.Token);
    }

    public async Task EvaluateAndQueueAsync(LocationPoint currentLocation, IReadOnlyCollection<Place> places, CancellationToken cancellationToken)
    {
        if (places.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (now - _lastTriggeredAt < Cooldown)
        {
            return;
        }

        var candidates = places
            .Where(p => p.Latitude != 0 && p.Longitude != 0)
            .Select(p =>
            {
                var distance = GeofenceHelper.CalculateHaversineDistance(
                    currentLocation.Latitude,
                    currentLocation.Longitude,
                    p.Latitude,
                    p.Longitude);

                return new { Poi = p, Distance = distance };
            })
            .Where(x => x.Distance <= x.Poi.TriggerRadius)
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        var selected = candidates
            .Select(x => new
            {
                x.Poi,
                x.Distance,
                Score = BuildPriorityScore(x.Poi, x.Distance)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Distance)
            .ThenByDescending(x => x.Poi.Priority)
            .First();

        if (_lastPoiId == selected.Poi.Id && now - _lastTriggeredAt < TimeSpan.FromMinutes(3))
        {
            return;
        }

        await _queue.Writer.WriteAsync(new QueuedPoi(selected.Poi, selected.Distance, now), cancellationToken);
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in _queue.Reader.ReadAllAsync(cancellationToken))
        {
            _lastTriggeredAt = DateTimeOffset.UtcNow;
            _lastPoiId = item.Poi.Id;
            OnResolved?.Invoke(this, item);

            try
            {
                await Task.Delay(Cooldown, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private static double BuildPriorityScore(Place poi, double distance)
    {
        var priorityNorm = Math.Clamp(poi.Priority / 10d, 0d, 1d);
        var radius = Math.Max(1, poi.TriggerRadius);
        var proximityNorm = 1d - Math.Clamp(distance / radius, 0d, 1d);
        return (0.65d * priorityNorm) + (0.35d * proximityNorm);
    }

    public async ValueTask DisposeAsync()
    {
        if (_processorCts == null)
        {
            return;
        }

        _processorCts.Cancel();
        _processorCts.Dispose();
        _processorCts = null;
        _queue.Writer.TryComplete();
        await Task.CompletedTask;
    }
}

public readonly record struct LocationPoint(double Latitude, double Longitude);
public readonly record struct QueuedPoi(Place Poi, double DistanceMeters, DateTimeOffset EnqueuedAt);
