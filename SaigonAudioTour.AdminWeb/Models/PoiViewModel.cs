using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.AdminWeb.Models
{
    public class PoiViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên địa điểm không được để trống")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public double Lat { get; set; }
        public double Lng { get; set; }

        public int Radius { get; set; }
        public string? Image { get; set; }
        public int Priority { get; set; }
        public bool IsStopStation { get; set; }
        public int NarrationCount { get; set; } // Số lượng bài thuyết minh đang có
    }
}