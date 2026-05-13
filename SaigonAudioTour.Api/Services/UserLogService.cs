using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.DTOs.UserLogs;
using SaigonAudioTour.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public class UserLogService : IUserLogService
    {
        private readonly ApplicationDbContext _context;

        public UserLogService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<PoiStatisticDto>> GetTopListenedPoisAsync(int top = 5)
        {
            return await _context.UserLogs
                .AsNoTracking()
                .Where(log => log.POIId != null && log.EventType == "Nghe Audio")
                .GroupBy(log => new { log.POIId, log.POI.Name })
                .Select(group => new PoiStatisticDto
                {
                    PoiId = group.Key.POIId.Value,
                    PoiName = group.Key.Name ?? "Trạm đã bị xóa",
                    TotalListens = group.Count(),
                    AverageDurationSeconds = group.Average(log => log.DurationSeconds ?? 0)
                })
                .OrderByDescending(stat => stat.TotalListens)
                .Take(top)
                .ToListAsync();
        }

        public async Task<HeatmapDataDto> GetHeatmapDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? groupBy = "poi")
        {
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            var query = _context.UserLogs
                .AsNoTracking()
                .Where(log => log.POIId != null && log.EventType == "Nghe Audio" 
                    && log.CreatedAt >= startDate && log.CreatedAt <= endDate);

            var heatmapItems = await query
                .GroupBy(log => new { log.POIId, log.POI.Name })
                .Select(group => new HeatmapItemDto
                {
                    PoiId = group.Key.POIId.Value,
                    PoiName = group.Key.Name ?? "Trạm đã bị xóa",
                    VisitCount = group.Count(),
                    AvgDuration = (int)group.Average(log => log.DurationSeconds ?? 0)
                })
                .OrderByDescending(x => x.VisitCount)
                .ToListAsync();

            return new HeatmapDataDto { HeatmapData = heatmapItems };
        }

        public async Task RecordListenEventAsync(string userId, int poiId, int duration)
        {
            _context.UserLogs.Add(new UserLog
            {
                UserId = userId,
                POIId = poiId,
                EventType = "Nghe Audio",
                DurationSeconds = duration,
                CreatedAt = System.DateTime.Now
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Generic event logging for SignalR telemetry
        /// </summary>
        public async Task LogEventAsync(string userId, int? poiId, string eventType, double? lat = null, double? lng = null, long? durationSeconds = null)
        {
            _context.UserLogs.Add(new UserLog
            {
                UserId = userId,
                POIId = poiId,
                EventType = eventType,
                Lat = lat,
                Lng = lng,
                DurationSeconds = durationSeconds,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
    }
}