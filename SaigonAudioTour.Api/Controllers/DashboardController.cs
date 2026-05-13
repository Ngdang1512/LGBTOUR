using SaigonAudioTour.Api.DTOs.UserLogs;
using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using SaigonAudioTour.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUserLogService _userLogService;
        private readonly ApplicationDbContext _context;

        public DashboardController(IUserLogService userLogService, ApplicationDbContext context)
        {
            _userLogService = userLogService;
            _context = context;
        }

        // GET: api/dashboard/top-pois
        [HttpGet("top-pois")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PoiStatisticDto>>> GetTopPois([FromQuery] int top = 5)
        {
            var result = await _userLogService.GetTopListenedPoisAsync(top);
            return Ok(result);
        }

        // GET: api/dashboard/heatmap
        [HttpGet("heatmap")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HeatmapDataDto>> GetHeatmap(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null, 
            [FromQuery] string? groupBy = "poi")
        {
            var result = await _userLogService.GetHeatmapDataAsync(startDate, endDate, groupBy);
            return Ok(result);
        }

        // GET: api/dashboard/revenue-summary
        [HttpGet("revenue-summary")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RevenueSummaryDto>> GetRevenueSummary([FromQuery] int days = 7)
        {
            days = Math.Clamp(days, 1, 90);

            var utcNow = DateTime.UtcNow;
            var startDate = utcNow.Date.AddDays(-(days - 1));

            var completedTransactions = await _context.PaymentTransactions
                .AsNoTracking()
                .Where(t => t.Status == PaymentTransactionStatus.Completed)
                .Select(t => new
                {
                    t.Amount,
                    t.PlanId,
                    t.UserId,
                    EventAt = t.ConfirmedAt ?? t.UpdatedAt
                })
                .Where(t => t.EventAt >= startDate && t.EventAt <= utcNow)
                .ToListAsync();

            var totalUsers = await _context.Users.AsNoTracking().CountAsync();
            var activePremiumUsers = await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.SubscriptionStatus == "premium");

            var totalRevenue = completedTransactions.Sum(t => t.Amount);
            var completedPayments = completedTransactions.Count;
            var ticketsSold = completedPayments;

            var premiumBuyers = completedTransactions
                .Where(t => !string.IsNullOrWhiteSpace(t.UserId))
                .Select(t => t.UserId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var conversionRate = totalUsers > 0
                ? Math.Round(activePremiumUsers * 100m / totalUsers, 2)
                : 0m;

            var arpu = premiumBuyers > 0
                ? Math.Round(totalRevenue / premiumBuyers, 2)
                : 0m;

            var byDay = completedTransactions
                .GroupBy(t => t.EventAt.Date)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Revenue = g.Sum(x => x.Amount),
                        PremiumPurchases = g.Count(x => string.Equals(x.PlanId, "premium", StringComparison.OrdinalIgnoreCase)),
                        CompletedPayments = g.Count()
                    });

            var trend = new List<RevenueTrendItemDto>();
            for (var i = 0; i < days; i++)
            {
                var d = startDate.AddDays(i).Date;
                if (byDay.TryGetValue(d, out var item))
                {
                    trend.Add(new RevenueTrendItemDto
                    {
                        Date = d.ToString("yyyy-MM-dd"),
                        Revenue = item.Revenue,
                        PremiumPurchases = item.PremiumPurchases,
                        CompletedPayments = item.CompletedPayments
                    });
                }
                else
                {
                    trend.Add(new RevenueTrendItemDto
                    {
                        Date = d.ToString("yyyy-MM-dd"),
                        Revenue = 0,
                        PremiumPurchases = 0,
                        CompletedPayments = 0
                    });
                }
            }

            return Ok(new RevenueSummaryDto
            {
                StartDate = startDate,
                EndDate = utcNow,
                TotalRevenue = totalRevenue,
                TicketsSold = ticketsSold,
                CompletedPayments = completedPayments,
                ActivePremiumUsers = activePremiumUsers,
                TotalUsers = totalUsers,
                PremiumBuyers = premiumBuyers,
                ConversionRate = conversionRate,
                Arpu = arpu,
                Trend = trend
            });
        }

        // POST: api/dashboard/record-listen
        [HttpPost("record-listen")]
        [AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("write_public")]
        public async Task<IActionResult> RecordListen([FromQuery] string userId, [FromQuery] int poiId, [FromQuery] int duration)
        {
            // Validate để chống spam / data poisoning
            if (string.IsNullOrWhiteSpace(userId) || userId.Length > 128)
                return BadRequest(new { message = "userId không hợp lệ." });
            if (poiId <= 0)
                return BadRequest(new { message = "poiId phải là số dương." });
            if (duration < 0 || duration > 7200) // tối đa 2 giờ
                return BadRequest(new { message = "duration không hợp lệ (0–7200 giây)." });

            await _userLogService.RecordListenEventAsync(userId, poiId, duration);
            return Ok();
        }

        // POST: api/dashboard/heartbeat
        [HttpPost("heartbeat")]
        [AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("write_public")]
        public async Task<IActionResult> RecordHeartbeat([FromQuery] string? userId, [FromQuery] string? deviceId)
        {
            if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(deviceId))
                return BadRequest(new { message = "Thiếu userId hoặc deviceId." });

            // Sanitize và giới hạn độ dài
            var normalizedUserId   = (userId   ?? "anonymous").Trim()[..Math.Min((userId   ?? "anonymous").Trim().Length, 128)];
            var normalizedDeviceId = (deviceId ?? "unknown"  ).Trim()[..Math.Min((deviceId ?? "unknown"  ).Trim().Length, 128)];
            var sessionKey = $"{normalizedUserId}|{normalizedDeviceId}";

            _context.UserLogs.Add(new UserLog
            {
                UserId    = sessionKey,
                POIId     = null,
                EventType = "Heartbeat",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // GET: api/dashboard/active-users
        [HttpGet("active-users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ActiveUsersSnapshotDto>> GetActiveUsers([FromQuery] int minutes = 5, [FromQuery] int top = 20)
        {
            minutes = Math.Clamp(minutes, 1, 120);
            top = Math.Clamp(top, 1, 100);

            var now = DateTime.UtcNow;
            var threshold = now.AddMinutes(-minutes);

            var activeGrouped = await _context.UserLogs
                .AsNoTracking()
                .Where(l => l.EventType == "Heartbeat" && l.CreatedAt >= threshold)
                .GroupBy(l => l.UserId)
                .Select(g => new
                {
                    SessionKey = g.Key,
                    LastSeenAt = g.Max(x => x.CreatedAt),
                    HeartbeatCount = g.Count()
                })
                .ToListAsync();

            var activeLogs = activeGrouped
                .Select(x =>
                {
                    var parts = (x.SessionKey ?? string.Empty).Split('|', 2, StringSplitOptions.None);
                    return new ActiveUserItemDto
                    {
                        SessionKey = x.SessionKey ?? string.Empty,
                        UserId = parts.Length > 0 ? parts[0] : string.Empty,
                        DeviceId = parts.Length > 1 ? parts[1] : string.Empty,
                        LastSeenAt = x.LastSeenAt,
                        HeartbeatCount = x.HeartbeatCount
                    };
                })
                .OrderByDescending(x => x.LastSeenAt)
                .ToList();

            var activeDevices = activeLogs.Count;
            var activeAccounts = activeLogs
                .Select(x => x.UserId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return Ok(new ActiveUsersSnapshotDto
            {
                WindowMinutes = minutes,
                AsOfUtc = now,
                ActiveUsers = activeDevices,
                ActiveDevices = activeDevices,
                ActiveAccounts = activeAccounts,
                Items = activeLogs.Take(top).ToList()
            });
        }
    }

    public class RevenueSummaryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TicketsSold { get; set; }
        public int CompletedPayments { get; set; }
        public int ActivePremiumUsers { get; set; }
        public int TotalUsers { get; set; }
        public int PremiumBuyers { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal Arpu { get; set; }
        public List<RevenueTrendItemDto> Trend { get; set; } = new();
    }

    public class RevenueTrendItemDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int PremiumPurchases { get; set; }
        public int CompletedPayments { get; set; }
    }

    public class ActiveUsersSnapshotDto
    {
        public int WindowMinutes { get; set; }
        public DateTime AsOfUtc { get; set; }
        // Backward-compatible field. Represents active devices.
        public int ActiveUsers { get; set; }
        public int ActiveDevices { get; set; }
        public int ActiveAccounts { get; set; }
        public List<ActiveUserItemDto> Items { get; set; } = new();
    }

    public class ActiveUserItemDto
    {
        public string SessionKey { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime LastSeenAt { get; set; }
        public int HeartbeatCount { get; set; }
    }
}