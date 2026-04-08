// (Dùng để trả dữ liệu cho Frontend)
namespace LGBTOUR.Api.DTOs.Pois
{
    public class PoiDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int Priority { get; set; }
        // Chỉ trả về số lượng bài thuyết minh để Admin biết quán này đã có audio chưa
        public int NarrationCount { get; set; }
    }
}