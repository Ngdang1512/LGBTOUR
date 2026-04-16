using SaigonAudioTour.Mobile.Models;
using System.Net.Http.Json;

namespace SaigonAudioTour.Mobile.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
            {
                username = email,
                password
            });

            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AuthResult>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<AuthResult?> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
            {
                fullName,
                email,
                password
            });

            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AuthResult>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserProfile?> GetUserProfileAsync(string email)
    {
        try
        {
            var encodedEmail = Uri.EscapeDataString(email ?? string.Empty);
            var response = await _httpClient.GetAsync($"api/auth/profile?email={encodedEmail}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserProfile>();
        }
        catch
        {
            return null;
        }
    }

    public sealed class AuthResult
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
