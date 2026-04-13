using System.ComponentModel.DataAnnotations;

namespace SaigonAudioTour.Api.DTOs.Pois
{
    public class CreatePoiDto
    {
        [Required(ErrorMessage = "Tên địa điểm không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }

        // Bán kính (mét) để xe buýt nhận diện tự động bật audio
        public int Radius { get; set; }
        public int Priority { get; set; }

        // MỚI: Đánh dấu có phải trạm dừng hay không
        public bool IsStopStation { get; set; }
    }
}