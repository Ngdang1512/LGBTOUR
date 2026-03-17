namespace LGBTOUR.AdminWeb.ViewModels
{
    public class TourItineraryViewModel
    {
        public int POI_Id { get; set; }
        public string POIName { get; set; } = string.Empty;
        public bool IsSelected { get; set; } // Checkbox: Điểm này có nằm trong Tour không?
        public int DisplayOrder { get; set; } // Thứ tự xe bus đi qua
    }
}