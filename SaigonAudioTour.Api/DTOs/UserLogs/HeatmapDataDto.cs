namespace SaigonAudioTour.Api.DTOs.UserLogs
{
    public class HeatmapDataDto
    {
        public List<HeatmapItemDto> HeatmapData { get; set; } = new();
    }

    public class HeatmapItemDto
    {
        public int PoiId { get; set; }
        public string? PoiName { get; set; }
        public int VisitCount { get; set; }
        public int AvgDuration { get; set; }
    }
}
