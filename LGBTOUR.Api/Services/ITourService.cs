using LGBTOUR.Api.DTOs.Tours;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public interface ITourService
    {
        Task<TourDetailDto> CreateTourAsync(CreateTourDto dto);
        Task<TourDetailDto?> GetTourByIdAsync(int id);
        Task<bool> AddPoiToTourAsync(int tourId, AddPoiToTourDto dto);
    }
}