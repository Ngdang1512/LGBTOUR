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

    public string PremiumStatusBackground => IsPremium ? "#DCFCE7" : "White";
    public string PremiumStatusColor => IsPremium ? "#166534" : "#0F172A";
    public string PremiumHintText => IsPremium
        ? "🎉 Premium đang hoạt động. Bạn đã mở toàn bộ tính năng cao cấp."
        : "Chọn gói bên dưới để nâng cấp tài khoản.";

    private bool _isPremium;
    public bool IsPremium
    {
        get => _isPremium;
        set
        {
            _isPremium = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowPurchaseSection));
            OnPropertyChanged(nameof(PremiumStatusBackground));
            OnPropertyChanged(nameof(PremiumStatusColor));
            OnPropertyChanged(nameof(PremiumHintText));
            OnPropertyChanged(nameof(ShowOrderSection));
        }
    }

    public bool ShowPurchaseSection => !IsPremium;

    public string OrderInfoText { get; set; } = string.Empty;

    private bool _isOrderVisible;
    public bool IsOrderVisible
    {
        get => _isOrderVisible;
        set
        {
            _isOrderVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowOrderSection));
        }
    }

    public bool ShowOrderSection => ShowPurchaseSection && IsOrderVisible;

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
        IsPremium = status.IsPremium;
        PremiumStatusText = BuildPremiumStatusText(status);

        if (status.IsPremium)
        {
            CurrentOrder = null;
            IsOrderVisible = false;
            OrderInfoText = string.Empty;
            OnPropertyChanged(nameof(CurrentOrder));
            OnPropertyChanged(nameof(OrderInfoText));
        }

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

        var purchasedPlan = Plans.FirstOrDefault(p => p.Id == CurrentOrder.PlanId);
        _apiService.SavePremiumStatusLocal(_userId, CurrentOrder.PlanId, purchasedPlan?.DurationDays ?? 30);

        var status = await _apiService.GetPremiumStatusAsync(_userId);
        IsPremium = status.IsPremium;
        PremiumStatusText = BuildPremiumStatusText(status);

        IsOrderVisible = false;
        CurrentOrder = null;
        OrderInfoText = string.Empty;

        OnPropertyChanged(nameof(PremiumStatusText));
        OnPropertyChanged(nameof(CurrentOrder));
        OnPropertyChanged(nameof(OrderInfoText));
        await DisplayAlertAsync("Thành công", "Tài khoản đã được nâng cấp Premium.", "OK");
    }

    private static string BuildPremiumStatusText(PremiumStatus status)
    {
        if (!status.IsPremium || !status.PremiumUntil.HasValue)
        {
            return "Trạng thái: Free";
        }

        var untilLocal = status.PremiumUntil.Value.ToLocalTime();
        var daysLeft = Math.Max(0, (int)Math.Ceiling((untilLocal.Date - DateTime.Now.Date).TotalDays));
        var planName = GetPlanDisplayName(status.PlanId);

        return $"Trạng thái: {planName} • còn {daysLeft} ngày (đến {untilLocal:dd/MM/yyyy})";
    }

    private static string GetPlanDisplayName(string? planId)
    {
        if (string.IsNullOrWhiteSpace(planId)) return "Premium";

        return planId.ToLowerInvariant() switch
        {
            "premium_month" => "Premium tháng",
            "premium_year" => "Premium năm",
            "pro_month" => "Pro tháng",
            _ => planId
        };
    }
}
