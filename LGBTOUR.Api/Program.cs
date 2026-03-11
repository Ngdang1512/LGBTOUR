var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// DỮ LIỆU ĐỘNG (Sau này Admin sẽ nhập từ trang Web)
var places = new[]
{
    new {
        Id = 1, Name = "Nhà hát Thành phố", Location = "Quận 1, TP. Hồ Chí Minh",
        ImageUrl = "https://images.unsplash.com/photo-1583417319070-4a69db38a482?w=600",
        Rating = "4.8", Category = "Popular", Latitude = 10.776562, Longitude = 106.703140,
        TriggerRadius = 50, Priority = 1, 
        TtsScript = "Chào mừng bạn đến với Nhà hát Thành phố Hồ Chí Minh. Đây là một công trình kiến trúc mang đậm phong cách Gothic của Pháp."
    },
    new {
        Id = 2, Name = "Phở Gia Truyền", Location = "Bát Đàn, Hà Nội",
        ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=600",
        Rating = "4.9", Category = "Food", Latitude = 21.0319, Longitude = 105.8485,
        TriggerRadius = 30, Priority = 2, 
        TtsScript = "Phở Gia Truyền Bát Đàn, một trong những biểu tượng ẩm thực lâu đời không thể bỏ qua khi đến thủ đô."
    },
    new {
        Id = 3, Name = "Bưu điện Trung tâm", Location = "Quận 1, TP. Hồ Chí Minh",
        ImageUrl = "https://images.unsplash.com/photo-1599827552599-eadf5af3cba8?w=600",
        Rating = "4.7", Category = "Popular", Latitude = 10.779836, Longitude = 106.699990,
        TriggerRadius = 60, Priority = 3, 
        TtsScript = "Trước mắt bạn là Bưu điện Trung tâm Sài Gòn, kiệt tác được thiết kế bởi vị kiến trúc sư nổi tiếng Gustave Eiffel."
    },
    new {
        Id = 4, Name = "Tràng Tiền Plaza", Location = "Hoàn Kiếm, Hà Nội",
        ImageUrl = "https://images.unsplash.com/photo-1567401893414-76b7b1e5a7a5?w=600",
        Rating = "4.5", Category = "Shopping", Latitude = 21.0253, Longitude = 105.8524,
        TriggerRadius = 40, Priority = 4, 
        TtsScript = "Tràng Tiền Plaza, trung tâm mua sắm xa xỉ và mang đậm dấu ấn lịch sử nằm ngay cạnh Hồ Gươm."
    }
};

// ĐƯỜNG DẪN 1: Dành cho Trang chủ (Lấy tất cả mọi địa điểm)
app.MapGet("/api/places", () => places);

// ĐƯỜNG DẪN 2: Dành cho Trang Khám Phá (Chỉ lọc ra những điểm ở TP.HCM để xếp thành 1 tour)
app.MapGet("/api/tours/hcm-route", () => places.Where(p => p.Location.Contains("Hồ Chí Minh")));

// MỞ KHÓA MẠNG: Dùng 0.0.0.0 thay vì localhost để cho phép máy ảo Android chui vào lấy dữ liệu
app.Run("http://0.0.0.0:5100");