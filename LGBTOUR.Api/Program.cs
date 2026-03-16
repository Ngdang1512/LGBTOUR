var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// LỘ TRÌNH XE BUÝT 2 TẦNG TPHCM (TOUR ĐÊM)
var places = new[]
{
    new {
        Id = 1, Name = "Nhà Hát Thành Phố", Location = "07 Công Trường Lam Sơn, Quận 1",
        ImageUrl = "https://images.unsplash.com/photo-1583417319070-4a69db38a482?w=600",
        Rating = "4.8", Category = "Khởi hành", Latitude = 10.776562, Longitude = 106.703140,
        TriggerRadius = 100, Priority = 1, 
        TtsScript = "Chào mừng quý khách đến với hành trình khám phá thành phố về đêm. Điểm khởi hành của chúng ta là Nhà hát Thành phố Hồ Chí Minh, một kiệt tác kiến trúc Gothic đặc trưng của Pháp được xây dựng từ năm 1898."
    },
    new {
        Id = 2, Name = "Phố đi bộ Nguyễn Huệ", Location = "Đường Nguyễn Huệ, Quận 1",
        ImageUrl = "https://images.unsplash.com/photo-1588667368688-66236b28fc05?w=600",
        Rating = "4.9", Category = "Điểm tham quan", Latitude = 10.774211, Longitude = 106.704230,
        TriggerRadius = 100, Priority = 2, 
        TtsScript = "Chúng ta đang đi ngang qua Phố đi bộ Nguyễn Huệ, quảng trường sầm uất và hiện đại bậc nhất thành phố. Nơi đây là điểm hẹn văn hóa quen thuộc của người dân Sài Gòn mỗi dịp cuối tuần."
    },
    new {
        Id = 3, Name = "Bến Nhà Rồng", Location = "01 Nguyễn Tất Thành, Quận 4",
        ImageUrl = "https://images.unsplash.com/photo-1600171285090-e51c89db1f2b?w=600",
        Rating = "4.7", Category = "Lịch sử", Latitude = 10.768434, Longitude = 106.706856,
        TriggerRadius = 150, Priority = 3, 
        TtsScript = "Bên tay phải quý khách là Bến Nhà Rồng. Nơi đây vào ngày 5 tháng 6 năm 1911, người thanh niên Nguyễn Tất Thành đã ra đi tìm đường cứu nước. Tòa nhà nổi bật với kiến trúc Á Âu kết hợp."
    },
    new {
        Id = 4, Name = "Tượng đài Trần Hưng Đạo", Location = "Bến Bạch Đằng, Quận 1",
        ImageUrl = "https://images.unsplash.com/photo-1650365749449-757e721f42d2?w=600",
        Rating = "4.6", Category = "Điểm tham quan", Latitude = 10.774520, Longitude = 106.706175,
        TriggerRadius = 100, Priority = 4, 
        TtsScript = "Mời quý khách hướng mắt về Tượng đài Quốc công Tiết chế Hưng Đạo Đại Vương Trần Quốc Tuấn đang oai phong chỉ tay về phía sông Sài Gòn, nhắc nhở về những chiến công lẫy lừng trong lịch sử."
    },
    new {
        Id = 5, Name = "Cầu Thủ Thiêm 1", Location = "Sông Sài Gòn",
        ImageUrl = "https://images.unsplash.com/photo-1629854728701-a15d5e56230f?w=600",
        Rating = "4.5", Category = "Cảnh quan", Latitude = 10.785055, Longitude = 106.716945,
        TriggerRadius = 200, Priority = 5, 
        TtsScript = "Xe đang đưa chúng ta qua Cầu Thủ Thiêm 1. Từ đây, quý khách có thể phóng tầm mắt ngắm nhìn toàn cảnh trung tâm thành phố lung linh ánh đèn phản chiếu xuống mặt sông Sài Gòn thơ mộng."
    },
    new {
        Id = 6, Name = "Cầu Ba Son (Thủ Thiêm 2)", Location = "Tôn Đức Thắng, Quận 1",
        ImageUrl = "https://images.unsplash.com/photo-1662963364955-460eb11a681c?w=600",
        Rating = "4.8", Category = "Kiến trúc", Latitude = 10.781615, Longitude = 106.708819,
        TriggerRadius = 200, Priority = 6, 
        TtsScript = "Chúng ta đang di chuyển trên Cầu Ba Son, biểu tượng kiến trúc mới của thành phố với thiết kế dây văng hình nón độc đáo, nối liền trung tâm Quận 1 và khu đô thị mới Thủ Thiêm."
    },
    new {
        Id = 7, Name = "Diamond Plaza", Location = "34 Lê Duẩn, Quận 1",
        ImageUrl = "https://images.unsplash.com/photo-1604543501712-4f3df9c0ccf3?w=600",
        Rating = "4.5", Category = "Mua sắm", Latitude = 10.782782, Longitude = 106.698755,
        TriggerRadius = 80, Priority = 7, 
        TtsScript = "Phía trước là trung tâm thương mại Diamond Plaza, một trong những khu mua sắm phức hợp sang trọng và lâu đời nhất tại thành phố, mang đậm phong cách kiến trúc hiện đại xen lẫn cổ điển."
    },
    new {
        Id = 8, Name = "Hồ Con Rùa", Location = "Công trường Quốc Tế, Quận 3",
        ImageUrl = "https://images.unsplash.com/photo-1616790956557-61e29e9d6d58?w=600",
        Rating = "4.6", Category = "Điểm tham quan", Latitude = 10.782803, Longitude = 106.695982,
        TriggerRadius = 80, Priority = 8, 
        TtsScript = "Quý khách đang ngắm nhìn Hồ Con Rùa, vòng xoay giao thông nổi tiếng và cũng là biểu tượng văn hóa gắn liền với ký ức của biết bao thế hệ học sinh, sinh viên Sài Gòn."
    },
    new {
        Id = 9, Name = "Dinh Thống Nhất", Location = "135 Nam Kỳ Khởi Nghĩa, Quận 1",
        ImageUrl = "https://images.unsplash.com/photo-1595914652253-1577717469a5?w=600",
        Rating = "4.9", Category = "Lịch sử", Latitude = 10.777085, Longitude = 106.695325,
        TriggerRadius = 150, Priority = 9, 
        TtsScript = "Bên trái quý khách là Dinh Thống Nhất, chứng nhân lịch sử quan trọng đánh dấu thời khắc hòa bình, thống nhất đất nước vào ngày 30 tháng 4 năm 1975."
    },
    new {
        Id = 10, Name = "Bưu điện & Nhà thờ Đức Bà", Location = "Công xã Paris, Quận 1",
        ImageUrl = "https://images.unsplash.com/photo-1599827552599-eadf5af3cba8?w=600",
        Rating = "4.9", Category = "Kiến trúc", Latitude = 10.779836, Longitude = 106.699990,
        TriggerRadius = 100, Priority = 10, 
        TtsScript = "Điểm tham quan cuối cùng trước khi kết thúc hành trình là cụm kiến trúc tuyệt mỹ: Bưu điện Trung tâm Sài Gòn và Vương cung Thánh đường Đức Bà, biểu tượng tráng lệ của Hòn ngọc Viễn Đông."
    }
};

// Cung cấp danh sách lộ trình qua API
app.MapGet("/api/places", () => places);
app.MapGet("/api/tours/hcm-route", () => places);

// Chạy server ở cổng 5100
app.Run("http://0.0.0.0:5100");