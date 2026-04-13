using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.DTOs.Tours
{
    public class UpdateTourDto
    {
        [Required(ErrorMessage = "Tên Tuyến không được để trống")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, 1000)]
        public int EstimatedTimeMinutes { get; set; }

        public double TicketPrice { get; set; }
        public double TotalDistanceKm { get; set; }
    }
}