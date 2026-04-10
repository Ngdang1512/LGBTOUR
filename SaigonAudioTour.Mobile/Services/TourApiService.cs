// SaigonAudioTour.Mobile/Services/TourApiService.cs
using SaigonAudioTour.Mobile.Models;
using System.Net.Http.Json;
using System.Globalization;

namespace SaigonAudioTour.Mobile.Services;

public class TourApiService
{
    private const string PremiumPlanKeyPrefix = "PremiumPlan_";
    private const string PremiumUntilKeyPrefix = "PremiumUntil_";

    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7289/")
    };

    public TourApiService()
    {
    }

    private static string GetPremiumPlanKey(string userId) => $"{PremiumPlanKeyPrefix}{userId}";
    private static string GetPremiumUntilKey(string userId) => $"{PremiumUntilKeyPrefix}{userId}";

    public void SavePremiumStatusLocal(string userId, string planId, int durationDays)
    {
        var safeDays = Math.Max(1, durationDays);
        var until = DateTime.UtcNow.AddDays(safeDays);

        Preferences.Set(GetPremiumPlanKey(userId), string.IsNullOrWhiteSpace(planId) ? "premium_month" : planId);
        Preferences.Set(GetPremiumUntilKey(userId), until.ToString("O", CultureInfo.InvariantCulture));
    }

    public void ClearPremiumStatusLocal(string userId)
    {
        Preferences.Remove(GetPremiumPlanKey(userId));
        Preferences.Remove(GetPremiumUntilKey(userId));
    }

    private PremiumStatus GetLocalPremiumStatus(string userId)
    {
        var savedPlanId = Preferences.Get(GetPremiumPlanKey(userId), "free");
        var savedUntilText = Preferences.Get(GetPremiumUntilKey(userId), string.Empty);

        if (DateTime.TryParse(savedUntilText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var premiumUntil)
            && premiumUntil > DateTime.UtcNow)
        {
            return new PremiumStatus
            {
                UserId = userId,
                IsPremium = true,
                PlanId = string.IsNullOrWhiteSpace(savedPlanId) ? "premium_month" : savedPlanId,
                PremiumUntil = premiumUntil
            };
        }

        ClearPremiumStatusLocal(userId);
        return new PremiumStatus
        {
            UserId = userId,
            IsPremium = false,
            PlanId = "free",
            PremiumUntil = null
        };
    }

    // 1. Dữ liệu giả lập cho bản đồ: tuyến xe buýt 2 tầng Quận 1
    public async Task<List<Place>> GetRoutePlacesAsync()
    {
        await Task.Delay(300);

        return new List<Place>
        {
            new Place
            {
                Id = 1,
                Name = "Chợ Bến Thành",
                Location = "Phường Bến Thành, Quận 1, TP.HCM",
                Latitude = 10.77252,
                Longitude = 106.69805,
                ImageUrl = "chobenthanh.jpg",
                Rating = "4.8",
                Category = "Điểm tham quan",
                TriggerRadius = 70,
                Priority = 10,
                TtsScript = "Chợ Bến Thành, tọa lạc tại trung tâm Quận 1, TP.HCM, là biểu tượng lịch sử và văn hóa hơn 100 năm tuổi của Sài Gòn. Xây dựng từ năm 1912, chợ nổi bật với kiến trúc cửa Nam có đồng hồ đặc trưng. Nơi đây không chỉ là điểm giao thương nhộn nhịp với đa dạng hàng hóa, ẩm thực mà còn là điểm đến không thể bỏ qua của du khách."
            },
            new Place
            {
                Id = 2,
                Name = "Nhà hát Thành phố",
                Location = "Số 7 Công trường Lam Sơn, Quận 1, TP.HCM",
                Latitude = 10.77662,
                Longitude = 106.70326,
                ImageUrl = "nhahat.jpg",
                Rating = "4.7",
                Category = "Điểm tham quan",
                TriggerRadius = 65,
                Priority = 9,
                TtsScript = "Nhà hát Thành phố Hồ Chí Minh (số 7 Công trường Lam Sơn, Quận 1) là di tích kiến trúc nghệ thuật cấp Quốc gia, được khánh thành năm 1900. Với phong cách kiến trúc Pháp cổ kính (Gothic), nơi đây được ví như \"thánh đường nghệ thuật\", chuyên tổ chức các buổi hòa nhạc, sân khấu đẳng cấp và là biểu tượng văn hóa lâu đời của TP.HCM."
            },
            new Place
            {
                Id = 3,
                Name = "Trụ sở Ủy ban Nhân dân TP.HCM",
                Location = "Số 86 Lê Thánh Tôn, Quận 1, TP.HCM",
                Latitude = 10.77695,
                Longitude = 106.70192,
                ImageUrl = "ubnd.jpg",
                Rating = "4.6",
                Category = "Điểm tham quan",
                TriggerRadius = 65,
                Priority = 8,
                TtsScript = "Trụ sở Ủy ban Nhân dân TP.HCM (City Hall) tọa lạc tại số 86 Lê Thánh Tôn, Quận 1, là công trình kiến trúc Pháp cổ kính biểu tượng của Sài Gòn. Xây dựng từ 1898-1909, tòa nhà nổi bật với phong cách Baroque, màu vàng kem rực rỡ và nằm ở cuối đại lộ Nguyễn Huệ, trở thành điểm tham quan check-in không thể bỏ qua."
            },
            new Place
            {
                Id = 4,
                Name = "Dinh Độc Lập",
                Location = "135 Nam Kỳ Khởi Nghĩa, Quận 1, TP.HCM",
                Latitude = 10.77713,
                Longitude = 106.69531,
                ImageUrl = "dinhdoclap.jpg",
                Rating = "4.6",
                Category = "Điểm tham quan",
                TriggerRadius = 60,
                Priority = 7,
                TtsScript = "Dinh Độc Lập (hay Hội trường Thống Nhất) tại Quận 1, TP.HCM là Di tích quốc gia đặc biệt, biểu tượng của hòa bình và thống nhất dân tộc. Được kiến trúc sư Ngô Viết Thụ thiết kế, công trình kết hợp hài hòa kiến trúc hiện đại và triết lý phương Đông. Nơi đây từng là cơ quan đầu não của chính quyền Sài Gòn và chứng kiến thời khắc lịch sử xe tăng húc đổ cổng chính ngày 30/4/1975"
            },
            new Place
            {
                Id = 5,
                Name = "Nhà thờ Đức Bà",
                Location = "Số 1 Công xã Paris, Quận 1, TP.HCM",
                Latitude = 10.77978,
                Longitude = 106.69902,
                ImageUrl = "nhatho.jpg",
                Rating = "4.7",
                Category = "Điểm tham quan",
                TriggerRadius = 58,
                Priority = 6,
                TtsScript = "Nhà thờ Đức Bà Sài Gòn (số 1 Công xã Paris, Quận 1) là biểu tượng kiến trúc Pháp đặc trưng, tồn tại hơn 140 năm tại TP.HCM. Khởi công năm 1877 và khánh thành năm 1880, công trình nổi bật với gạch đỏ Marseille không phai màu, hai tháp chuông cao 57m và lối kiến trúc Roman-Gothic cổ kính. Đây là Vương cung thánh đường nổi tiếng, điểm du lịch văn hóa, lịch sử và tín ngưỡng không thể bỏ qua."
            },
            new Place
            {
                Id = 6,
                Name = "Bưu điện Thành phố",
                Location = "Số 2 Công xã Paris, Quận 1, TP.HCM",
                Latitude = 10.77992,
                Longitude = 106.69928,
                ImageUrl = "buudien.jpg",
                Rating = "4.6",
                Category = "Điểm tham quan",
                TriggerRadius = 55,
                Priority = 7,
                TtsScript = "Bưu điện Trung tâm Sài Gòn (2 Công xã Paris, Quận 1) là công trình kiến trúc Pháp tiêu biểu được xây dựng từ năm 1886-1891. Đây là bưu điện lâu đời nhất Đông Nam Á, nổi bật với phong cách kiến trúc kết hợp Âu - Á độc đáo, mái vòm rộng và sàn đá cẩm thạch, do kiến trúc sư Gustave Eiffel thiết kế."
            },
            new Place
            {
                Id = 7,
                Name = "Bảo tàng Chứng tích Chiến tranh",
                Location = "28 Võ Văn Tần, Quận 3, TP.HCM",
                Latitude = 10.77948,
                Longitude = 106.69216,
                ImageUrl = "baotangchungtich.jpg",
                Rating = "4.6",
                Category = "Điểm tham quan",
                TriggerRadius = 52,
                Priority = 7,
                TtsScript = "Bảo tàng Chứng tích Chiến tranh (War Remnants Museum) tại 28 Võ Văn Tần, Q.3, TP.HCM là điểm đến không thể bỏ qua, chuyên trưng bày tư liệu, hình ảnh và hiện vật về tội ác và hậu quả chiến tranh xâm lược Việt Nam. Thành lập năm 1975, bảo tàng lưu giữ hàng nghìn chứng tích, bao gồm cả không gian ngoài trời với máy bay, xe tăng, nhằm tuyên truyền về hòa bình, tình đoàn kết dân tộc."
            },
            new Place
            {
                Id = 8,
                Name = "Bảo tàng Lịch sử",
                Location = "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM",
                Latitude = 10.78812,
                Longitude = 106.70432,
                ImageUrl = "baotanglichsu.jpg",
                Rating = "4.7",
                Category = "Điểm tham quan",
                TriggerRadius = 52,
                Priority = 6,
                TtsScript = "Bảo tàng Lịch sử (History Museum) là không gian lưu giữ, trưng bày các hiện vật quý giá, tái hiện sinh động dòng chảy văn hóa và các cột mốc quan trọng của một dân tộc. Đây là điểm đến văn hóa, giáo dục lý tưởng, nơi kết nối quá khứ với hiện tại, giúp người tham quan hiểu rõ hơn về cội nguồn."
            },
            new Place
            {
                Id = 9,
                Name = "Thảo Cầm Viên Sài Gòn",
                Location = "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM",
                Latitude = 10.78710,
                Longitude = 106.70560,
                ImageUrl = "thaocamvien.jpg",
                Rating = "4.8",
                Category = "Điểm tham quan",
                TriggerRadius = 58,
                Priority = 6,
                TtsScript = "Thảo Cầm Viên Sài Gòn (Saigon Zoo), thành lập năm 1864, là một trong những vườn thú lâu đời nhất thế giới và là \"lá phổi xanh\" hơn 20ha giữa Quận 1, TP.HCM. Nơi đây bảo tồn hơn 1.000 cá thể động vật và 2.000 cây xanh, là điểm tham quan, giáo dục tự nhiên lý tưởng với giá vé từ 40.000đ - 60.000đ."
            },
            new Place
            {
                Id = 10,
                Name = "Saigon Skydeck",
                Location = "Tầng 49, Bitexco Financial Tower, Quận 1, TP.HCM",
                Latitude = 10.77168,
                Longitude = 106.70411,
                ImageUrl = "skydeck.jpg",
                Rating = "4.6",
                Category = "Điểm tham quan",
                TriggerRadius = 60,
                Priority = 5,
                TtsScript = "Saigon Skydeck, tọa lạc tại tầng 49 của tòa tháp Bitexco Financial Tower (Quận 1, TP.HCM), là đài quan sát đầu tiên và nổi tiếng tại Việt Nam, mang đến tầm nhìn toàn cảnh 360 độ từ độ cao 178m. Nơi đây cho phép du khách ngắm nhìn sông Sài Gòn và thành phố, kết hợp trải nghiệm văn hóa truyền thống qua tranh mộc bản."
            }
        };
    }

    // 2. Dữ liệu danh sách Trang chủ: đồng bộ tuyến xe buýt 2 tầng Quận 1
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
            Email = "admin@SAT.com",
            AvatarUrl = "https://example.com/avatar.jpg" // Có thể thay bằng ảnh local nếu cần
        };
    }

    // 4. Dữ liệu cho đồ án: tuyến xe buýt 2 tầng Quận 1
    public async Task<List<Place>> GetProjectPlacesAsync()
    {
        return await GetRoutePlacesAsync();
    }

    // 5. Subscription plans cho màn hình nâng cấp
    public async Task<List<PremiumPlan>> GetPremiumPlansAsync()
    {
        try
        {
            var plans = await _httpClient.GetFromJsonAsync<List<PremiumPlan>>("api/subscription/plans");
            if (plans is { Count: > 0 }) return plans;
        }
        catch
        {
            // fallback mock
        }

        return new List<PremiumPlan>
        {
            new() { Id = "premium_month", Name = "Premium tháng", Price = 49000, Currency = "VND", DurationDays = 30, Features = "Mở toàn bộ audio + heatmap nâng cao + không quảng cáo" },
            new() { Id = "premium_year", Name = "Premium năm", Price = 299000, Currency = "VND", DurationDays = 365, Features = "Toàn bộ tính năng Premium, tiết kiệm chi phí" },
            new() { Id = "pro_month", Name = "Pro tháng", Price = 99000, Currency = "VND", DurationDays = 30, Features = "Premium + AI gợi ý lịch trình + thống kê cá nhân" }
        };
    }

    public async Task<PaymentOrder?> CreateUpgradeOrderAsync(string userId, string planId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/subscription/create-order", new { userId, planId });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PaymentOrder>();
        }
        catch
        {
            var fakeOrderId = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var qrPayload = $"lgbtour://pay?orderId={fakeOrderId}&plan={planId}";
            return new PaymentOrder
            {
                OrderId = fakeOrderId,
                UserId = userId,
                PlanId = planId,
                Amount = planId == "premium_year" ? 299000 : (planId == "pro_month" ? 99000 : 49000),
                Currency = "VND",
                Status = "pending",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                QrImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=320x320&data={Uri.EscapeDataString(qrPayload)}"
            };
        }
    }

    public async Task<PaymentOrder?> GetOrderStatusAsync(string orderId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PaymentOrder>($"api/subscription/order-status/{orderId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> MarkOrderPaidAsync(string orderId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/subscription/mark-paid/{orderId}", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return true; // fallback cho demo
        }
    }

    public async Task<PremiumStatus> GetPremiumStatusAsync(string userId)
    {
        try
        {
            var status = await _httpClient.GetFromJsonAsync<PremiumStatus>($"api/subscription/user/{userId}/status");
            if (status != null)
            {
                if (status.IsPremium && status.PremiumUntil.HasValue && status.PremiumUntil.Value > DateTime.UtcNow)
                {
                    var inferredDuration = Math.Max(1, (int)Math.Ceiling((status.PremiumUntil.Value - DateTime.UtcNow).TotalDays));
                    SavePremiumStatusLocal(userId, status.PlanId, inferredDuration);
                }
                else
                {
                    ClearPremiumStatusLocal(userId);
                }

                return status;
            }
        }
        catch
        {
            // fallback
        }

        return GetLocalPremiumStatus(userId);
    }
}