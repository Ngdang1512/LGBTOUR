using System.Text.Json;
using SaigonAudioTour.Api.Models.Realtime;
using StackExchange.Redis;

namespace SaigonAudioTour.Api.Services;

public sealed class RedisUserActivityStore : IUserActivityStore, IDisposable
{
    private const string DevicesSetKey = "sat:activity:devices";
    private const string ActivityHashKey = "sat:activity:latest";
    private const string LastSeenHashKey = "sat:activity:lastseen";
    private const string ConnectionPrefix = "sat:activity:conn:";

    private readonly ILogger<RedisUserActivityStore> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisUserActivityStore(string connectionString, ILogger<RedisUserActivityStore> logger)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect(connectionString);
        _db = _redis.GetDatabase();
    }

    public int OnlineCount
    {
        get
        {
            try
            {
                return (int)_db.SetLength(DevicesSetKey);
            }
            catch
            {
                return 0;
            }
        }
    }

    public void Upsert(ActivityTelemetryDto telemetry)
    {
        if (telemetry == null || string.IsNullOrWhiteSpace(telemetry.DeviceId))
        {
            return;
        }

        telemetry.Timestamp = telemetry.Timestamp == default ? DateTimeOffset.UtcNow : telemetry.Timestamp;

        try
        {
            var payload = JsonSerializer.Serialize(telemetry);
            var nowUnix = telemetry.Timestamp.ToUnixTimeSeconds();

            var tran = _db.CreateTransaction();
            _ = tran.HashSetAsync(ActivityHashKey, telemetry.DeviceId, payload);
            _ = tran.HashSetAsync(LastSeenHashKey, telemetry.DeviceId, nowUnix);
            _ = tran.SetAddAsync(DevicesSetKey, telemetry.DeviceId);
            tran.Execute();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to upsert activity for {DeviceId}", telemetry.DeviceId);
        }
    }

    public IReadOnlyCollection<ActivityTelemetryDto> GetSnapshot()
    {
        try
        {
            var rows = _db.HashGetAll(ActivityHashKey);
            var result = new List<ActivityTelemetryDto>(rows.Length);

            foreach (var row in rows)
            {
                if (!row.Value.HasValue)
                {
                    continue;
                }

                try
                {
                    var json = row.Value.ToString();
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        continue;
                    }

                    var item = JsonSerializer.Deserialize<ActivityTelemetryDto>(json);
                    if (item != null)
                    {
                        result.Add(item);
                    }
                }
                catch
                {
                    // skip malformed payload
                }
            }

            return result
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read activity snapshot from Redis");
            return Array.Empty<ActivityTelemetryDto>();
        }
    }

    public void LinkConnection(string connectionId, string deviceId)
    {
        if (string.IsNullOrWhiteSpace(connectionId) || string.IsNullOrWhiteSpace(deviceId))
        {
            return;
        }

        try
        {
            _db.StringSet(GetConnectionKey(connectionId), deviceId, TimeSpan.FromHours(2));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to link connection {ConnectionId}", connectionId);
        }
    }

    public void RemoveByConnection(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            return;
        }

        try
        {
            var key = GetConnectionKey(connectionId);
            var deviceId = _db.StringGet(key);
            _db.KeyDelete(key);

            if (!deviceId.HasValue)
            {
                return;
            }

            var tran = _db.CreateTransaction();
            _ = tran.SetRemoveAsync(DevicesSetKey, deviceId);
            _ = tran.HashDeleteAsync(ActivityHashKey, deviceId);
            _ = tran.HashDeleteAsync(LastSeenHashKey, deviceId);
            tran.Execute();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove activity by connection {ConnectionId}", connectionId);
        }
    }

    public int RemoveInactive(TimeSpan staleAfter)
    {
        if (staleAfter <= TimeSpan.Zero)
        {
            return 0;
        }

        var threshold = DateTimeOffset.UtcNow.Subtract(staleAfter).ToUnixTimeSeconds();
        var removed = 0;

        try
        {
            var lastSeenEntries = _db.HashGetAll(LastSeenHashKey);

            foreach (var entry in lastSeenEntries)
            {
                var raw = entry.Value.ToString();
                if (!long.TryParse(raw, out var lastSeenUnix) || lastSeenUnix >= threshold)
                {
                    continue;
                }

                var deviceId = entry.Name;
                var tran = _db.CreateTransaction();
                _ = tran.SetRemoveAsync(DevicesSetKey, deviceId);
                _ = tran.HashDeleteAsync(ActivityHashKey, deviceId);
                _ = tran.HashDeleteAsync(LastSeenHashKey, deviceId);

                if (tran.Execute())
                {
                    removed++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup stale activity from Redis");
        }

        return removed;
    }

    private static string GetConnectionKey(string connectionId) => $"{ConnectionPrefix}{connectionId}";

    public void Dispose()
    {
        _redis.Dispose();
    }
}
