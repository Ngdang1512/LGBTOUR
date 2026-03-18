namespace LGBTOUR.Api.DTOs
{
    public class UserLogDto
    {
        // ID của điện thoại hoặc tài khoản người dùng
        public string UserId { get; set; } = string.Empty;

        // ID của địa điểm đang nghe
        public int POI_Id { get; set; }

        // Tọa độ lúc người dùng đứng nghe (dùng để vẽ Heatmap)
        public double Lat { get; set; }
        public double Lng { get; set; }

        // Thời gian đã nghe (giây)
        public long DurationSeconds { get; set; }
    }
}