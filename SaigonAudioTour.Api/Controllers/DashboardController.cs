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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PoiStatisticDto>>> GetTopPois([FromQuery] int top = 5)
        {
            var result = await _userLogService.GetTopListenedPoisAsync(top);
            return Ok(result);
        }

        // GET: api/dashboard/heatmap
        [HttpGet("heatmap")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [AllowAnonymous] // MỞ CỬA cho Mobile App gọi ngầm không cần đăng nhập
        public async Task<IActionResult> RecordListen([FromQuery] string userId, [FromQuery] int poiId, [FromQuery] int duration)
        {
            await _userLogService.RecordListenEventAsync(userId, poiId, duration);
            return Ok();
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
}