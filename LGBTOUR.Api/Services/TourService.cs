using LGBTOUR.Api.Data;
using LGBTOUR.Api.DTOs.Tours;
using LGBTOUR.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public class TourService : ITourService
    {
        private readonly ApplicationDbContext _context;

        public TourService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TourDetailDto> CreateTourAsync(CreateTourDto dto)
        {
            var tour = new Tour
            {
                Name = dto.Name,
                Description = dto.Description,
                EstimatedTimeMinutes = dto.EstimatedTimeMinutes
            };

            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();

            return new TourDetailDto
            {
                Id = tour.Id,
                Name = tour.Name,
                Description = tour.Description,
                EstimatedTimeMinutes = tour.EstimatedTimeMinutes
            };
        }

        // LOGIC QUAN TRỌNG NHẤT: Lấy Tour kèm danh sách quán ăn đã được sắp xếp
        public async Task<TourDetailDto?> GetTourByIdAsync(int id)
        {
            var tour = await _context.Tours
                .Include(t => t.TourPOIs)            // Kết nối bảng trung gian
                    .ThenInclude(tp => tp.POI)       // Từ bảng trung gian lấy ra thông tin Quán ăn
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return null;

            return new TourDetailDto
            {
                Id = tour.Id,
                Name = tour.Name,
                Description = tour.Description,
                EstimatedTimeMinutes = tour.EstimatedTimeMinutes,
                // Chuyển đổi và Sắp xếp các quán ăn theo DisplayOrder tăng dần
                Pois = tour.TourPOIs
                    .OrderBy(tp => tp.DisplayOrder)
                    .Select(tp => new TourPoiItemDto
                    {
                        PoiId = tp.POI.Id,
                        PoiName = tp.POI.Name,
                        DisplayOrder = tp.DisplayOrder
                    }).ToList()
            };
        }

        // Logic Admin thêm 1 quán ăn vào Tour
        public async Task<bool> AddPoiToTourAsync(int tourId, AddPoiToTourDto dto)
        {
            // Kiểm tra Tour và POI có tồn tại không
            var tourExists = await _context.Tours.AnyAsync(t => t.Id == tourId);
            var poiExists = await _context.POIs.AnyAsync(p => p.Id == dto.PoiId);

            if (!tourExists || !poiExists) return false;

            // Kiểm tra xem quán này đã có trong tour chưa để tránh trùng lặp
            var existingLink = await _context.TourPOIs
                .FirstOrDefaultAsync(tp => tp.TourId == tourId && tp.POI_Id == dto.PoiId);

            if (existingLink != null)
            {
                // Nếu có rồi thì chỉ cập nhật lại thứ tự (DisplayOrder)
                existingLink.DisplayOrder = dto.DisplayOrder;
            }
            else
            {
                // Nếu chưa có thì tạo mới liên kết
                var newTourPoi = new TourPOI
                {
                    TourId = tourId,
                    POI_Id = dto.PoiId,
                    DisplayOrder = dto.DisplayOrder
                };
                _context.TourPOIs.Add(newTourPoi);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}