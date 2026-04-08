using LGBTOUR.Api.DTOs.Auth;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginDto dto);
    }
}