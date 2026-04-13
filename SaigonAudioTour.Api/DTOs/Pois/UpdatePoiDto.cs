using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.DTOs.Pois
{
    public class UpdatePoiDto
    {
        [Required(ErrorMessage = "Tên địa điểm không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Radius { get; set; }
        public int Priority { get; set; }
        public bool IsStopStation { get; set; }
    }
}