using LGBTOUR.Api.DTOs.Narrations;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public interface INarrationService
    {
        Task<NarrationDto?> AddNarrationAsync(CreateNarrationDto dto);
    }
}