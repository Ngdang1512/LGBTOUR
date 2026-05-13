using SaigonAudioTour.Api.DTOs.Tours;
using SaigonAudioTour.Api.Entities;
using System.Collections.Generic; // Bổ sung thư viện này để dùng IEnumerable
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public interface ITourService
    {
        // >>> ĐÂY LÀ DÒNG VỪA ĐƯỢC THÊM VÀO ĐỂ FIX LỖI <<<
        Task<IEnumerable<TourDetailDto>> GetAllToursAsync();
        // Thêm dòng này vào
      

        Task<TourDetailDto> CreateTourAsync(CreateTourDto dto);
        Task<TourDetailDto?> GetTourByIdAsync(int id);
        Task<bool> AddPoiToTourAsync(int tourId, AddPoiToTourDto dto);
        Task<bool> UpdateRouteAsync(int tourId, List<int> orderedPoiIds);
        Task<bool> UpdateTourAsync(int id, UpdateTourDto dto);
        Task<bool> DeleteTourAsync(int id);
    }
}