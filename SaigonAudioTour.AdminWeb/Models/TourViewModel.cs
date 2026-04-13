using System.ComponentModel.DataAnnotations;

namespace LGBTOUR.AdminWeb.Models
{
    public class TourViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tuyến không được để trống")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int EstimatedTimeMinutes { get; set; } // Phút
        public double TicketPrice { get; set; } // Giá vé
        public double TotalDistanceKm { get; set; } // Tổng độ dài (Km)
    }
}