using LGBTOUR.Api.Data;
using LGBTOUR.Api.DTOs.UserLogs;
using LGBTOUR.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
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
    }
}