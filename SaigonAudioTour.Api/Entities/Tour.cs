using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.Entities
{
    public class Tour
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int EstimatedTimeMinutes { get; set; } // Thời gian dự kiến (Phút)

        
        // 2 TRƯỜNG MỚI THÊM CHO XE BUÝT 2 TẦNG
        
        public double TicketPrice { get; set; } // Giá vé
        public double TotalDistanceKm { get; set; } // Tổng chiều dài tuyến (Km)

        // Navigation property
        public ICollection<TourPOI> TourPOIs { get; set; } = new List<TourPOI>();
    }
}