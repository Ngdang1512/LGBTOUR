using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.DTOs.Tours;
using SaigonAudioTour.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public class TourService : ITourService
    {
        private readonly ApplicationDbContext _context;

        public TourService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<TourDetailDto>> GetAllToursAsync()
        {
            return await _context.Tours.AsNoTracking()
                .Select(t => new TourDetailDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    EstimatedTimeMinutes = t.EstimatedTimeMinutes,
                    TicketPrice = t.TicketPrice,
                    TotalDistanceKm = t.TotalDistanceKm
                }).ToListAsync();
        }

        public async Task<TourDetailDto?> GetTourByIdAsync(int id)
        {
            var tour = await _context.Tours.AsNoTracking()
                .Include(t => t.TourPOIs).ThenInclude(tp => tp.POI)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return null;

            return new TourDetailDto
            {
                Id = tour.Id,
                Name = tour.Name,
                Description = tour.Description,
                EstimatedTimeMinutes = tour.EstimatedTimeMinutes,
                TicketPrice = tour.TicketPrice,
                TotalDistanceKm = tour.TotalDistanceKm,
                Pois = tour.TourPOIs.OrderBy(tp => tp.DisplayOrder)
                    .Select(tp => new TourPoiItemDto
                    {
                        PoiId = tp.POI.Id,
                        PoiName = tp.POI.Name,
                        DisplayOrder = tp.DisplayOrder
                    }).ToList()
            };
        }

        public async Task<TourDetailDto> CreateTourAsync(CreateTourDto dto)
        {
            var tour = new Tour
            {
                Name = dto.Name,
                Description = dto.Description,
                EstimatedTimeMinutes = dto.EstimatedTimeMinutes,
                TicketPrice = dto.TicketPrice,
                TotalDistanceKm = dto.TotalDistanceKm
            };
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            return await GetTourByIdAsync(tour.Id) ?? new TourDetailDto();
        }

        public async Task<bool> UpdateTourAsync(int id, UpdateTourDto dto)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return false;

            tour.Name = dto.Name; tour.Description = dto.Description;
            tour.EstimatedTimeMinutes = dto.EstimatedTimeMinutes;
            tour.TicketPrice = dto.TicketPrice; tour.TotalDistanceKm = dto.TotalDistanceKm;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTourAsync(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return false;
            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddPoiToTourAsync(int tourId, AddPoiToTourDto dto)
        {
            var tourExists = await _context.Tours.AnyAsync(t => t.Id == tourId);
            var poiExists = await _context.POIs.AnyAsync(p => p.Id == dto.PoiId);
            if (!tourExists || !poiExists) return false;

            var existingLink = await _context.TourPOIs
                .FirstOrDefaultAsync(tp => tp.TourId == tourId && tp.POI_Id == dto.PoiId);

            if (existingLink != null) existingLink.DisplayOrder = dto.DisplayOrder;
            else
            {
                _context.TourPOIs.Add(new TourPOI
                {
                    TourId = tourId,
                    POI_Id = dto.PoiId,
                    DisplayOrder = dto.DisplayOrder
                });
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}