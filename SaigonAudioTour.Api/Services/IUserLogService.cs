using SaigonAudioTour.Api.DTOs.UserLogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public interface IUserLogService
    {
        // Lấy Top 5 hoặc Top 10 quán ăn được nghe nhiều nhất
        Task<IEnumerable<PoiStatisticDto>> GetTopListenedPoisAsync(int top = 5);

        // Lấy heatmap data cho analytics dashboard
        Task<HeatmapDataDto> GetHeatmapDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? groupBy = "poi");

        // (Tuỳ chọn) API cho Mobile App gọi để ghi nhận mỗi khi user bấm Play Audio
        Task RecordListenEventAsync(string userId, int poiId, int duration);
    }
}