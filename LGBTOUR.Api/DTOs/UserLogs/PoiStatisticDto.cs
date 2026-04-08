namespace LGBTOUR.Api.DTOs.UserLogs
{
    public class PoiStatisticDto
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public int TotalListens { get; set; } // Tổng số lượt nghe
        public double AverageDurationSeconds { get; set; } // Nghe trung bình bao nhiêu giây
    }
}