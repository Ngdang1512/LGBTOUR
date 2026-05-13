using SaigonAudioTour.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace SaigonAudioTour.Api.Services;

public sealed class ActivityPresenceCleanupService : BackgroundService
{
    private readonly IUserActivityStore _store;
    private readonly IHubContext<ActivityHub> _hubContext;
    private readonly ILogger<ActivityPresenceCleanupService> _logger;

    public ActivityPresenceCleanupService(
        IUserActivityStore store,
        IHubContext<ActivityHub> hubContext,
        ILogger<ActivityPresenceCleanupService> logger)
    {
        _store = store;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                var removed = _store.RemoveInactive(TimeSpan.FromSeconds(20));
                if (removed > 0)
                {
                    await _hubContext.Clients.Group("admins")
                        .SendAsync("OnlineCount", _store.OnlineCount, cancellationToken: stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // service stopping
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Presence cleanup loop failed.");
            }
        }
    }
}
