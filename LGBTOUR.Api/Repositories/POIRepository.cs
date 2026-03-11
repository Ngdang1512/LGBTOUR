using LGBTOUR.Api.Data;
using LGBTOUR.Api.Entities;
using LGBTOUR.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LGBTOUR.Api.Repositories
{
    public class POIRepository : IPOIRepository
    {
        private readonly ApplicationDbContext _context;

        public POIRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<POI>> GetAllAsync()
            => await _context.POIs.ToListAsync();

        public async Task<POI?> GetByIdAsync(int id)
            => await _context.POIs.FindAsync(id);

        public async Task AddAsync(POI poi)
        {
            await _context.POIs.AddAsync(poi);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(POI poi)
        {
            _context.POIs.Update(poi);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi != null)
            {
                _context.POIs.Remove(poi);
                await _context.SaveChangesAsync();
            }
        }
    }
}