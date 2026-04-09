namespace LGBTOUR.Api.DTOs.UserLogs
{
    public class PoiStatisticDto
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public int TotalListens { get; set; } // Trạm nào được khách nghe thuyết minh nhiều nhất
        public double AverageDurationSeconds { get; set; }
    }
}