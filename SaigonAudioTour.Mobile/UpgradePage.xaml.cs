using System.Collections.ObjectModel;
using SaigonAudioTour.Mobile.Models;
using SaigonAudioTour.Mobile.Services;

namespace SaigonAudioTour.Mobile;

public partial class UpgradePage : ContentPage
{
    private readonly TourApiService _apiService;
    private readonly string _userId = "demo-user";

    public ObservableCollection<PremiumPlan> Plans { get; set; } = new();
    public PremiumPlan? SelectedPlan { get; set; }
    public PaymentOrder? CurrentOrder { get; set; }

    public string PremiumStatusText { get; set; } = "Trạng thái: FREE";
    public string OrderInfoText { get; set; } = string.Empty;
    public bool IsOrderVisible { get; set; }

    public UpgradePage()
    {
        InitializeComponent();
        _apiService = new TourApiService();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var plans = await _apiService.GetPremiumPlansAsync();
        Plans = new ObservableCollection<PremiumPlan>(plans);
        SelectedPlan = Plans.FirstOrDefault();

        var status = await _apiService.GetPremiumStatusAsync(_userId);
        PremiumStatusText = status.IsPremium
            ? $"Trạng thái: PREMIUM ({status.PlanId}) đến {status.PremiumUntil:dd/MM/yyyy}"
            : "Trạng thái: FREE";

        OnPropertyChanged(nameof(Plans));
        OnPropertyChanged(nameof(SelectedPlan));
        OnPropertyChanged(nameof(PremiumStatusText));
    }

    private void OnPlanSelected(object? sender, SelectionChangedEventArgs e)
    {
        SelectedPlan = e.CurrentSelection.FirstOrDefault() as PremiumPlan;
        OnPropertyChanged(nameof(SelectedPlan));
    }

    private async void OnCreateOrderClicked(object sender, EventArgs e)
    {
        var plan = SelectedPlan ?? Plans.FirstOrDefault();
        if (plan == null)
        {
            await DisplayAlertAsync("Thông báo", "Chưa có gói thanh toán.", "OK");
            return;
        }

        CurrentOrder = await _apiService.CreateUpgradeOrderAsync(_userId, plan.Id);
        if (CurrentOrder == null)
        {
            await DisplayAlertAsync("Lỗi", "Không tạo được đơn thanh toán.", "OK");
            return;
        }

        OrderInfoText = $"Mã đơn: {CurrentOrder.OrderId}\nSố tiền: {CurrentOrder.Amount:N0} {CurrentOrder.Currency}\nHết hạn: {CurrentOrder.ExpiresAt:HH:mm dd/MM}";
        IsOrderVisible = true;

        OnPropertyChanged(nameof(CurrentOrder));
        OnPropertyChanged(nameof(OrderInfoText));
        OnPropertyChanged(nameof(IsOrderVisible));
    }

    private async void OnMarkPaidClicked(object sender, EventArgs e)
    {
        if (CurrentOrder == null) return;

        var ok = await _apiService.MarkOrderPaidAsync(CurrentOrder.OrderId);
        if (!ok)
        {
            await DisplayAlertAsync("Lỗi", "Không cập nhật được thanh toán.", "OK");
            return;
        }

        var status = await _apiService.GetPremiumStatusAsync(_userId);
        PremiumStatusText = status.IsPremium
            ? $"Trạng thái: PREMIUM ({status.PlanId}) đến {status.PremiumUntil:dd/MM/yyyy}"
            : "Trạng thái: FREE";

        OnPropertyChanged(nameof(PremiumStatusText));
        await DisplayAlertAsync("Thành công", "Tài khoản đã được nâng cấp Premium.", "OK");
    }
}
