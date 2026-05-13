using SaigonAudioTour.Api.DTOs.Auth;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto?> LoginAsync(LoginDto dto);
        Task<AuthResultDto?> AdminLoginAsync(LoginDto dto);
        Task<AuthResultDto?> RefreshAdminTokenAsync(string username);
        Task<AuthResultDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResultDto?> GetProfileByEmailAsync(string email);
    }
}