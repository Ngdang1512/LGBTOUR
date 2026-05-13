using SaigonAudioTour.Api.DTOs.Pois;
using Microsoft.AspNetCore.Http; // Bổ sung thư viện này để dùng IFormFile
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public interface IPoiService
    {
        Task<IEnumerable<PoiDto>> GetAllPoisAsync();
        Task<PoiDto> CreatePoiAsync(CreatePoiDto createPoiDto);

        // --- THÊM HÀM NÀY CHO TÍNH NĂNG UPLOAD ẢNH ---
        Task<bool> UploadImageAsync(int poiId, IFormFile imageFile);
        // Thêm API tìm trạm gần nhất, bây giờ nhận thêm tham số ngôn ngữ
        Task<NearbyPoiDto?> GetNearbyPoiAsync(double currentLat, double currentLng, string langcode);
        Task<bool> UpdatePoiAsync(int id, UpdatePoiDto dto);
        Task<bool> DeletePoiAsync(int id);
        Task<int> SeedDemoPoisAsync();
    }
}