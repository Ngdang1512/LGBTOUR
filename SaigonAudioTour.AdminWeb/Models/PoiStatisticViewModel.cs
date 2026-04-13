namespace SaigonAudioTour.AdminWeb.Models
{
    public class PoiStatisticViewModel
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public int TotalListens { get; set; }
        public double AverageDurationSeconds { get; set; }
    }
}