using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.DTOs.Tours
{
    public class CreateTourDto
    {
        [Required(ErrorMessage = "Tên Tuyến không được để trống")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int EstimatedTimeMinutes { get; set; }

        // MỚI: Dành cho xe buýt
        public double TicketPrice { get; set; }
        public double TotalDistanceKm { get; set; }
    }
}