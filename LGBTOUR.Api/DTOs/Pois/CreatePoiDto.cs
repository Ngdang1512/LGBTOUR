using System.ComponentModel.DataAnnotations;

namespace LGBTOUR.Api.DTOs.Pois
{
    public class CreatePoiDto
    {
        [Required(ErrorMessage = "Tên địa điểm không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Radius { get; set; }
        public int Priority { get; set; }
        // Lưu ý: Không có trường Id ở đây, vì Id do DB tự tạo.
        // Tạm thời chưa bắt upload Image vội, sẽ xử lý ở API riêng.
    }
}