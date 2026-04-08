using LGBTOUR.Api.Data;
using LGBTOUR.Api.DTOs.Pois;
using LGBTOUR.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public class PoiService : IPoiService
    {
        private readonly ApplicationDbContext _context;

        public PoiService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PoiDto>> GetAllPoisAsync()
        {
            // Lấy từ DB (Entities) và chuyển đổi bằng tay (Manual Mapping) sang DTO
            var pois = await _context.POIs
                .Include(p => p.Narrations) // Lấy kèm Narration để đếm số lượng audio
                .Select(p => new PoiDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Image = p.Image,
                    Priority = p.Priority,
                    NarrationCount = p.Narrations.Count // Logic đếm số bài thuyết minh
                })
                .ToListAsync();

            return pois;
        }

        public async Task<PoiDto> CreatePoiAsync(CreatePoiDto dto)
        {
            // 1. Chuyển DTO (từ Admin gửi lên) thành Entity
            var newPoi = new POI
            {
                Name = dto.Name,
                Description = dto.Description,
                Lat = dto.Lat,
                Lng = dto.Lng,
                Radius = dto.Radius,
                Priority = dto.Priority
            };

            // 2. Lưu vào Database
            _context.POIs.Add(newPoi);
            await _context.SaveChangesAsync();

            // 3. Trả về DTO cho Frontend biết là đã tạo thành công
            return new PoiDto
            {
                Id = newPoi.Id,
                Name = newPoi.Name,
                Description = newPoi.Description,
                Priority = newPoi.Priority,
                NarrationCount = 0
            };
        }
    }
}