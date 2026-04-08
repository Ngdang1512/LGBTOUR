using LGBTOUR.Api.DTOs.Pois;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public interface IPoiService
    {
        // Lấy danh sách tất cả quán ăn
        Task<IEnumerable<PoiDto>> GetAllPoisAsync();

        // Thêm một quán ăn mới
        Task<PoiDto> CreatePoiAsync(CreatePoiDto createPoiDto);
    }
}