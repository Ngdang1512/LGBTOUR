using System.Net.Http.Json;

namespace AdminApp.Services;

public class AdminAuthClient
{
    private readonly HttpClient _httpClient;

    public AdminAuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AdminLoginResult?> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/admin-login", new
            {
                username,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AdminLoginResult>();
        }
        catch
        {
            return null;
        }
    }

    public sealed class AdminLoginResult
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
