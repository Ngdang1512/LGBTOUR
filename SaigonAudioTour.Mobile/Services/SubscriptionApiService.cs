using SaigonAudioTour.Mobile.Models;
using System.Net.Http.Json;

namespace SaigonAudioTour.Mobile.Services;

public class SubscriptionApiService
{
    private readonly HttpClient _httpClient;

    public SubscriptionApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PremiumPlan>> GetPremiumPlansAsync()
    {
        try
        {
            var plans = await _httpClient.GetFromJsonAsync<List<PremiumPlan>>("api/subscription/plans") ?? new List<PremiumPlan>();
            return plans.Where(p => p.Id is "default" or "premium").ToList();
        }
        catch
        {
            return new List<PremiumPlan>();
        }
    }

    public async Task<PaymentOrder?> CreateUpgradeOrderAsync(string userId, string planId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/subscription/create-order", new { userId, planId });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PaymentOrder>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<PaymentOrder?> GetOrderStatusAsync(string orderId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PaymentOrder>($"api/subscription/order-status/{orderId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> MarkOrderPaidAsync(string orderId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/subscription/mark-paid/{orderId}", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string userId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/subscription/cancel/{userId}", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PremiumStatus?> GetPremiumStatusAsync(string userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PremiumStatus>($"api/subscription/user/{userId}/status");
        }
        catch
        {
            return null;
        }
    }
}
