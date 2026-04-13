using SaigonAudioTour.Api.DTOs.Narrations;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public interface INarrationService
    {
        Task<NarrationDto?> AddNarrationAsync(CreateNarrationDto dto);
    }
}