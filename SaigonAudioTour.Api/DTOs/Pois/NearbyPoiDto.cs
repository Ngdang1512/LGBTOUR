namespace SaigonAudioTour.Api.DTOs.Pois
{
    public class NearbyPoiDto
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public bool IsStopStation { get; set; }
        public double DistanceMeters { get; set; } // Cách trạm bao nhiêu mét
        public string? AudioUrl { get; set; } // Link file mp3 để phát
    }
}