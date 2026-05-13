using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.Devices.Sensors;
using SaigonAudioTour.Mobile.Services.Geofencing;
using System.Diagnostics;

namespace SaigonAudioTour.Mobile.Services.Realtime;

public sealed class ActivityReporterService : IAsyncDisposable
{
    private const string GuestDeviceIdKey = "Realtime_GuestDeviceId";
    private readonly GeofenceSessionState _geofenceState;
    private readonly HttpClient _httpClient;
    private HubConnection? _connection;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _loopCts;
    private bool _isRunning;

    public ActivityReporterService(GeofenceSessionState geofenceState, HttpClient httpClient)
    {
        _geofenceState = geofenceState;
        _httpClient = httpClient;
    }

    public async Task StartAsync(string? userId, CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return;
        }

        var deviceId = string.IsNullOrWhiteSpace(userId)
            ? GetOrCreateGuestDeviceId()
            : $"u-{userId}";

        var baseUrl = (_httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5117");

        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/activity")
            .WithAutomaticReconnect()
            .Build();

        _connection.Closed += ex =>
        {
            Debug.WriteLine($"[ActivityReporter] SignalR closed: {ex?.Message ?? "no error"}");
            return Task.CompletedTask;
        };

        _connection.Reconnecting += ex =>
        {
            Debug.WriteLine($"[ActivityReporter] SignalR reconnecting: {ex?.Message ?? "network issue"}");
            return Task.CompletedTask;
        };

        _connection.Reconnected += id =>
        {
            Debug.WriteLine($"[ActivityReporter] SignalR reconnected: {id}");
            return Task.CompletedTask;
        };

        await _connection.StartAsync(cancellationToken);
        Debug.WriteLine($"[ActivityReporter] Started -> {baseUrl}/hubs/activity");

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isRunning = true;

        _ = Task.Run(() => SendLoopAsync(deviceId, _loopCts.Token), _loopCts.Token);
    }

    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        _loopCts?.Cancel();
        _timer?.Dispose();

        if (_connection != null)
        {
            try
            {
                await _connection.StopAsync();
            }
            catch
            {
                // ignore shutdown exceptions
            }

            await _connection.DisposeAsync();
            _connection = null;
        }

        _loopCts?.Dispose();
        _loopCts = null;
    }

    private async Task SendLoopAsync(string deviceId, CancellationToken cancellationToken)
    {
        if (_connection == null || _timer == null)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                if (_connection.State != HubConnectionState.Connected)
                {
                    continue;
                }

                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(4)), cancellationToken);
                if (location == null)
                {
                    continue;
                }

                var activePoi = _geofenceState.ActivePoi;
                var narratingPoiId = Preferences.Default.Get("NarratingPlaceId", -1);
                var status = _geofenceState.ActivityStatus;

                if (string.IsNullOrWhiteSpace(status) || status == "idle")
                {
                    status = activePoi == null
                        ? "moving"
                        : (activePoi.Id == narratingPoiId ? "listening" : "viewing_detail");
                }

                await _connection.InvokeAsync("UpdateActivity", new
                {
                    DeviceId = deviceId,
                    SessionId = AppInfo.Current.VersionString,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Status = status,
                    PoiId = activePoi?.Id,
                    PoiName = activePoi?.Name,
                    Timestamp = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ActivityReporter] Send loop error: {ex.Message}");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private static string GetOrCreateGuestDeviceId()
    {
        var existing = Preferences.Default.Get(GuestDeviceIdKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var created = $"guest-{Guid.NewGuid():N}";
        Preferences.Default.Set(GuestDeviceIdKey, created);
        return created;
    }
}
