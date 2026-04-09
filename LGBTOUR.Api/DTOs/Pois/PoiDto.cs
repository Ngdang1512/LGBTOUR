// (Dùng để trả dữ liệu cho Frontend)
namespace LGBTOUR.Api.DTOs.Pois
{
    public class PoiDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }

        // Geographic coordinates (added to match POI entity)
        public double Lat { get; set; }
        public double Lng { get; set; }

        public int Priority { get; set; }
        public int Radius { get; set; }

        // MỚI: Đánh dấu đây là Trạm cho khách xuống (true) hay chỉ đi ngang qua (false)
        public bool IsStopStation { get; set; }

        public int NarrationCount { get; set; }
    }
}