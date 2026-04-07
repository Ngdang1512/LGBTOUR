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

    // 1. Dữ liệu giả lập cho bản đồ (đổi sang khu phố ẩm thực Quận 4)
    public async Task<List<Place>> GetRoutePlacesAsync()
    {
        await Task.Delay(300);

        return new List<Place>
        {
            new Place
            {
                Id = 1,
                Name = "Phố ẩm thực Vĩnh Khánh",
                Location = "Đường Vĩnh Khánh, Phường 8, Quận 4, TP.HCM",
                Latitude = 10.7609,
                Longitude = 106.7048,
                ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?q=80&w=800",
                Rating = "4.8",
                Category = "Ẩm thực",
                TriggerRadius = 60,
                Priority = 10,
                TtsScript = "Chào mừng bạn đến phố ẩm thực Vĩnh Khánh. Đây là tuyến ăn uống nổi bật của Quận 4, tập trung nhiều quán ốc, hải sản, lẩu nướng và các điểm ăn đêm rất nhộn nhịp."
            },
            new Place
            {
                Id = 2,
                Name = "Ốc Oanh Vĩnh Khánh",
                Location = "534 Đường Vĩnh Khánh, Phường 8, Quận 4, TP.HCM",
                Latitude = 10.7624,
                Longitude = 106.7043,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-oc-oanh-1707245308.jpg",
                Rating = "4.7",
                Category = "Ẩm thực",
                TriggerRadius = 45,
                Priority = 9,
                TtsScript = "Ốc Oanh là một địa chỉ nổi bật trên tuyến Vĩnh Khánh. Quán có không gian bình dân, phục vụ nhiều món ốc và hải sản với hương vị đậm đà, phù hợp trải nghiệm buổi tối."
            },
            new Place
            {
                Id = 3,
                Name = "Quán ốc Sáu Nở",
                Location = "128 Đường Vĩnh Khánh, Phường 8, Quận 4, TP.HCM",
                Latitude = 10.7606,
                Longitude = 106.7051,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-oc-sau-no-1707245308.jpg",
                Rating = "4.6",
                Category = "Ẩm thực",
                TriggerRadius = 40,
                Priority = 8,
                TtsScript = "Ốc Sáu Nở được nhiều thực khách chọn nhờ hải sản tươi và nước chấm đậm vị. Đây là điểm dừng phù hợp nếu bạn muốn khám phá thiên đường ốc Quận 4."
            },
            new Place
            {
                Id = 4,
                Name = "Quán Ốc Thảo",
                Location = "383 Đường Vĩnh Khánh, Phường 8, Quận 4, TP.HCM",
                Latitude = 10.7617,
                Longitude = 106.7046,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-oc-thao-1707245333.jpg",
                Rating = "4.6",
                Category = "Ẩm thực",
                TriggerRadius = 40,
                Priority = 7,
                TtsScript = "Ốc Thảo có không gian rộng, menu đa dạng và giá dễ tiếp cận với học sinh sinh viên. Quán phù hợp đi nhóm đông vào khung giờ tối."
            },
            new Place
            {
                Id = 5,
                Name = "Ốc Đào",
                Location = "123 Đường Vĩnh Khánh, Phường 10, Quận 4, TP.HCM",
                Latitude = 10.7604,
                Longitude = 106.7058,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-oc-dao-1707245308.jpg",
                Rating = "4.7",
                Category = "Ẩm thực",
                TriggerRadius = 42,
                Priority = 6,
                TtsScript = "Ốc Đào nổi tiếng với các món sốt đậm đà như trứng muối và xào me. Hương vị dễ ăn, phù hợp nhiều vùng miền và thường rất đông vào giờ cao điểm."
            },
            new Place
            {
                Id = 6,
                Name = "Quán Ốc Vũ",
                Location = "37 Đường Vĩnh Khánh, Phường 8, Quận 4, TP.HCM",
                Latitude = 10.7602,
                Longitude = 106.7050,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-oc-vu-1707245333.jpg",
                Rating = "4.6",
                Category = "Ẩm thực",
                TriggerRadius = 48,
                Priority = 7,
                TtsScript = "Ốc Vũ là quán mở cửa khuya, phù hợp các nhóm bạn muốn ăn đêm. Thực đơn hải sản phong phú, mức giá bình dân và không khí rất sôi động."
            },
            new Place
            {
                Id = 7,
                Name = "Lãng Quán",
                Location = "531 Đường Vĩnh Khánh, Phường 10, Quận 4, TP.HCM",
                Latitude = 10.7622,
                Longitude = 106.7041,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-lang-quan-1707245308.jpg",
                Rating = "4.6",
                Category = "Ẩm thực",
                TriggerRadius = 45,
                Priority = 7,
                TtsScript = "Lãng Quán chuyên lẩu nướng với thực đơn nhiều món và nước chấm đặc trưng. Đây là lựa chọn hợp lý cho nhóm muốn ăn no và ngồi lâu."
            },
            new Place
            {
                Id = 8,
                Name = "Ớt Xiêm Quán",
                Location = "568 Đường Vĩnh Khánh, Phường 10, Quận 4, TP.HCM",
                Latitude = 10.7626,
                Longitude = 106.7040,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-ot-xiem-quan-1707245333.jpg",
                Rating = "4.5",
                Category = "Ẩm thực",
                TriggerRadius = 42,
                Priority = 6,
                TtsScript = "Ớt Xiêm Quán có không gian ấm cúng, menu đa dạng từ thịt đến hải sản. Mức giá tầm trung, phù hợp các buổi gặp mặt bạn bè."
            },
            new Place
            {
                Id = 9,
                Name = "Chilli Lẩu Nướng Quán",
                Location = "232 Đường Vĩnh Khánh, Phường 10, Quận 4, TP.HCM",
                Latitude = 10.7610,
                Longitude = 106.7053,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-chilli-quan-1707245308.jpg",
                Rating = "4.5",
                Category = "Ẩm thực",
                TriggerRadius = 40,
                Priority = 6,
                TtsScript = "Chilli nổi bật với sốt nướng đậm vị và các món lẩu thơm ngon. Quán thường đông cuối tuần, nên đi sớm để có chỗ tốt."
            },
            new Place
            {
                Id = 10,
                Name = "A Fat Hot Pot",
                Location = "668 Đường Vĩnh Khánh, Phường 10, Quận 4, TP.HCM",
                Latitude = 10.7630,
                Longitude = 106.7039,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-a-fat-hot-pot-1707245308.jpg",
                Rating = "4.5",
                Category = "Ẩm thực",
                TriggerRadius = 40,
                Priority = 5,
                TtsScript = "A Fat Hot Pot mang phong cách lẩu đường phố Trung Hoa, nổi bật với các vị lẩu chua cay và topping đa dạng."
            },
            new Place
            {
                Id = 11,
                Name = "Sườn nướng ớt",
                Location = "712 Đường Vĩnh Khánh, Phường 10, Quận 4, TP.HCM",
                Latitude = 10.7633,
                Longitude = 106.7038,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-suon-nuong-1707245333.jpg",
                Rating = "4.5",
                Category = "Ẩm thực",
                TriggerRadius = 38,
                Priority = 5,
                TtsScript = "Sườn nướng ớt nổi tiếng với món sườn nướng muối ớt và các combo nướng tiết kiệm. Nhân viên thân thiện, phù hợp đi nhóm."
            },
            new Place
            {
                Id = 12,
                Name = "Tỷ Muội Quán",
                Location = "232/59 Đường Vĩnh Khánh, Phường 10, Quận 4, TP.HCM",
                Latitude = 10.7608,
                Longitude = 106.7056,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-ty-muoi-quan-1707245333.jpg",
                Rating = "4.4",
                Category = "Ẩm thực",
                TriggerRadius = 35,
                Priority = 5,
                TtsScript = "Tỷ Muội Quán là lựa chọn bình dân, hợp túi tiền học sinh sinh viên. Không gian phù hợp tụ họp bạn bè vào buổi tối."
            },
            new Place
            {
                Id = 13,
                Name = "Quán 3 Cô Tiên",
                Location = "39 Đường Vĩnh Khánh, Phường 8, Quận 4, TP.HCM",
                Latitude = 10.7603,
                Longitude = 106.7052,
                ImageUrl = "https://mia.vn/media/uploads/blog-du-lich/pho-am-thuc-vinh-khanh-3-co-tien-1707245307.jpg",
                Rating = "4.6",
                Category = "Ẩm thực",
                TriggerRadius = 45,
                Priority = 8,
                TtsScript = "Quán 3 Cô Tiên mở cửa đến gần sáng, menu đa dạng và giá hợp lý. Đây là điểm quen thuộc của nhiều bạn trẻ khi khám phá Vĩnh Khánh về đêm."
            }
        };
    }

    // 2. Dữ liệu danh sách Trang chủ: đồng bộ cùng phạm vi Quận 4
    public async Task<List<Place>> GetAllPlacesAsync()
    {
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

    // 4. Dữ liệu cho đồ án: dùng đúng dataset Quận 4, không lọc từ Quận 1 nữa
    public async Task<List<Place>> GetProjectPlacesAsync()
    {
        return await GetRoutePlacesAsync();
    }
}