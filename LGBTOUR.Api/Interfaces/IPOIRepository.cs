using LGBTOUR.Api.Entities;
namespace LGBTOUR.Api.Interfaces
{
    public interface IPOIRepository
    {
        Task<IEnumerable<POI>> GetAllAsync();
        Task<POI?> GetByIdAsync(int id);
        Task AddAsync(POI poi);
        Task UpdateAsync(POI poi);
        Task DeleteAsync(int id);
    }
}
