using System.Collections.Generic;

namespace SaigonAudioTour.Api.DTOs.Tours
{
    public class TourDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EstimatedTimeMinutes { get; set; } // Thời gian chạy hết tuyến (Phút)

        // MỚI: Dành cho xe buýt
        public double TicketPrice { get; set; }
        public double TotalDistanceKm { get; set; }

        // Danh sách các trạm xe buýt sẽ đi qua (đã sắp xếp đúng thứ tự)
        public List<TourPoiItemDto> Pois { get; set; } = new List<TourPoiItemDto>();
    }

    public class TourPoiItemDto
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } // Thứ tự trạm (1, 2, 3...)
    }
}