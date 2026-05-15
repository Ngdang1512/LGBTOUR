namespace SaigonAudioTour.AdminWeb.Models
{
    public class TourRouteViewModel
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public List<PoiItemViewModel> RoutePois { get; set; } = new();
        public List<PoiItemViewModel> AvailablePois { get; set; } = new();
    }

    public class PoiItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Dùng để deserialize phản hồi từ API GET api/tours/{id}
    public class TourDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<TourPoiResponse> Pois { get; set; } = new();
    }

    public class TourPoiResponse
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
