using System.Net.Http.Json;
using LGBTOUR.Mobile.Models;

namespace LGBTOUR.Mobile.Services;

public class TourApiService
{
    private readonly HttpClient _httpClient;

    public TourApiService()
    {
        _httpClient = new HttpClient();
        // Giả sử chạy trên máy ảo Android kết nối với API ở localhost
        _httpClient.BaseAddress = new Uri("http://10.0.2.2:5100/"); 
    }

    // 1. Dành cho MapPage (Đã làm)
    public async Task<List<Place>> GetRoutePlacesAsync()
    {
        try { return await _httpClient.GetFromJsonAsync<List<Place>>("api/tours/hcm-route") ?? new List<Place>(); }
        catch { return new List<Place>(); }
    }

    // 2. Dành cho MainPage: Lấy tất cả địa điểm
    public async Task<List<Place>> GetAllPlacesAsync()
    {
        try { 
            // API ví dụ: trả về toàn bộ địa điểm
            return await _httpClient.GetFromJsonAsync<List<Place>>("api/places") ?? new List<Place>(); 
        }
        catch { return new List<Place>(); }
    }

    // 3. Dành cho SettingsPage: Lấy thông tin tài khoản
    public async Task<UserProfile> GetUserProfileAsync()
    {
        try { 
            // API ví dụ: trả về thông tin user đang đăng nhập
            return await _httpClient.GetFromJsonAsync<UserProfile>("api/user/profile"); 
        }
        catch { return null; }
    }
}