// LGBTOUR.Mobile/Services/TourApiService.cs
using LGBTOUR.Mobile.Models;

namespace LGBTOUR.Mobile.Services;

public class TourApiService
{
    // Đã comment HttpClient để không gọi lên API nữa cho buổi demo
    // private readonly HttpClient _httpClient;

    public TourApiService()
    {
    }

    // 1. Dữ liệu giả lập cho bản đồ và GPS xe buýt 2 tầng
    public async Task<List<Place>> GetRoutePlacesAsync()
    {
        await Task.Delay(500); 
        return new List<Place>
        {
            new Place 
            { 
                Name = "Dinh Độc Lập", 
                Location = "10.776889, 106.695083", 
                ImageUrl = "https://images.unsplash.com/photo-1583417319070-4a69db38a482?q=80&w=800", 
                Rating = "4.9", 
                Category = "Di tích", 
                TtsScript = "Chào mừng quý khách đến với Dinh Độc Lập, công trình kiến trúc lịch sử mang tính biểu tượng của thành phố." 
            },
            new Place 
            { 
                Name = "Nhà thờ Đức Bà", 
                Location = "10.779785, 106.699018", 
                ImageUrl = "https://images.unsplash.com/photo-1548625361-ec85871e80f8?q=80&w=800", 
                Rating = "4.8", 
                Category = "Tôn giáo",
                TtsScript = "Phía trước quý khách là Nhà thờ Đức Bà Sài Gòn, tuyệt tác kiến trúc với hơn 140 năm tuổi."
            },
            new Place 
            { 
                Name = "Chợ Bến Thành", 
                Location = "10.772540, 106.698020", 
                ImageUrl = "https://images.unsplash.com/photo-1588614959060-4d144f28b207?q=80&w=800", 
                Rating = "4.7", 
                Category = "Mua sắm",
                TtsScript = "Chúng ta đang đi ngang qua Chợ Bến Thành, khu chợ sầm uất và nổi tiếng bậc nhất."
            }
        };
    }

    // 2. Dữ liệu giả lập cho danh sách Trang chủ
    public async Task<List<Place>> GetAllPlacesAsync()
    {
        await Task.Delay(500);
        return await GetRoutePlacesAsync(); 
    }

    // 3. Dữ liệu giả lập cho Trang cá nhân
    public async Task<UserProfile> GetUserProfileAsync()
    {
        await Task.Delay(300);
        return new UserProfile
        {
            FullName = "Hướng dẫn viên VIP",
            Email = "admin@lgbtour.com",
            AvatarUrl = "https://example.com/avatar.jpg" // Có thể thay bằng ảnh local nếu cần
        };
    }
}