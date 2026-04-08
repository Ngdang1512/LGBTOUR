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

        public UserLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PoiStatisticDto>> GetTopListenedPoisAsync(int top = 5)
        {
            // Logic: Lọc các log có POI -> Gom nhóm theo Quán ăn -> Đếm số lượng -> Sắp xếp giảm dần
            var stats = await _context.UserLogs
                .Where(log => log.POIId != null && log.EventType == "Nghe Audio")
                .GroupBy(log => new { log.POIId, log.POI.Name })
                .Select(group => new PoiStatisticDto
                {
                    PoiId = group.Key.POIId.Value,
                    PoiName = group.Key.Name ?? "Quán đã bị xóa",
                    TotalListens = group.Count(),
                    AverageDurationSeconds = group.Average(log => log.DurationSeconds ?? 0)
                })
                .OrderByDescending(stat => stat.TotalListens) // Quán nào nghe nhiều lên đầu
                .Take(top) // Chỉ lấy Top X quán
                .ToListAsync();

            return stats;
        }

        public async Task RecordListenEventAsync(string userId, int poiId, int duration)
        {
            var log = new UserLog
            {
                UserId = userId,
                POIId = poiId,
                EventType = "Nghe Audio",
                DurationSeconds = duration,
                CreatedAt = System.DateTime.Now
            };

            _context.UserLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}