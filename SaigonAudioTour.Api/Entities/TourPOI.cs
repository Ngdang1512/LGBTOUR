namespace SaigonAudioTour.Api.Entities
{
    public class TourPOI
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int POI_Id { get; set; }
        public int DisplayOrder { get; set; } // Thứ tự điểm đến trong tour

        public virtual Tour? Tour { get; set; }
        public virtual POI? POI { get; set; }
    }
}