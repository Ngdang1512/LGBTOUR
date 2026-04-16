using System.Collections.ObjectModel;
using SaigonAudioTour.Mobile.Models;
using SaigonAudioTour.Mobile.Services;

namespace SaigonAudioTour.Mobile;

public partial class UpgradePage : ContentPage
{
    private readonly SubscriptionApiService _apiService;
    private readonly string _userId;
    private bool _hasShownIntroModal;

    public ObservableCollection<PremiumPlan> Plans { get; set; } = new();
    public PremiumPlan? SelectedPlan { get; set; }
    public PaymentOrder? CurrentOrder { get; set; }

    public string PremiumStatusText { get; set; } = "Trạng thái: FREE";

    public string PremiumStatusBackground => IsPremium ? "#DCFCE7" : "White";
    public string PremiumStatusColor => IsPremium ? "#166534" : "#0F172A";
    public string PremiumHintText => IsPremium
        ? "🎉 Premium đang hoạt động. Bạn đã mở toàn bộ tính năng cao cấp."
        : "Chọn gói bên dưới để nâng cấp tài khoản.";

    public bool ShowCancelSection => IsPremium;

    private bool _isPremium;
    public bool IsPremium
    {
        get => _isPremium;
        set
        {
            _isPremium = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowPurchaseSection));
            OnPropertyChanged(nameof(ShowCancelSection));
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
        _apiService = IPlatformApplication.Current?.Services.GetService<SubscriptionApiService>()
            ?? throw new InvalidOperationException("SubscriptionApiService chưa được đăng ký DI.");
        _userId = Preferences.Get(StorageKeys.UserId, string.Empty);
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();

        if (!IsPremium && !_hasShownIntroModal && Plans.Count > 0)
        {
            _hasShownIntroModal = true;
            await ShowPremiumIntroModalAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        if (string.IsNullOrWhiteSpace(_userId))
        {
            await DisplayAlertAsync("Yêu cầu đăng nhập", "Vui lòng đăng nhập để nâng cấp gói.", "OK");
            await Navigation.PushAsync(new LoginPage());
            return;
        }

        var plans = await _apiService.GetPremiumPlansAsync();
        Plans = new ObservableCollection<PremiumPlan>(plans);
        SelectedPlan = Plans.FirstOrDefault(p => p.Id == "premium") ?? Plans.FirstOrDefault();

        var status = await _apiService.GetPremiumStatusAsync(_userId);
        if (status == null)
        {
            IsPremium = false;
            PremiumStatusText = "Trạng thái: Không thể lấy dữ liệu từ API";
        }
        else
        {
            IsPremium = status.IsPremium;
            PremiumStatusText = BuildPremiumStatusText(status);
        }

        if (status?.IsPremium == true)
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

    private async Task ShowPremiumIntroModalAsync()
    {
        var title = "✨ Premium";
        var message = "Mở khóa audio tự động, giao diện không quảng cáo và quyền lợi ưu tiên. Bạn có thể xem gói ngay bây giờ.";

        var result = await this.DisplayAlertAsync(title, message, "Xem gói", "Để sau");
        if (result)
        {
            IsOrderVisible = true;
        }
    }

    private void OnPlanSelected(object? sender, SelectionChangedEventArgs e)
    {
        SelectedPlan = e.CurrentSelection.FirstOrDefault() as PremiumPlan;
        OnPropertyChanged(nameof(SelectedPlan));
    }

    private async void OnCreateOrderClicked(object sender, EventArgs e)
    {
        if (IsPremium)
        {
            await DisplayAlertAsync("Premium", "Tài khoản của bạn đã là Premium rồi.", "OK");
            return;
        }

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

        EnsureQrImageUrl(CurrentOrder);

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
        var status = await _apiService.GetPremiumStatusAsync(_userId);
        if (status == null)
        {
            IsPremium = false;
            PremiumStatusText = "Trạng thái: Không thể lấy dữ liệu từ API";
        }
        else
        {
            IsPremium = status.IsPremium;
            PremiumStatusText = BuildPremiumStatusText(status);
        }

        if (!ok && status?.IsPremium != true)
        {
            await DisplayAlertAsync("Lỗi", "Không cập nhật được thanh toán.", "OK");
            return;
        }

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
            return $"Trạng thái: {GetStatusDisplayName(status.Status)}";
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
            "premium" => "Premium",
            "default" => "Mặc định",
            _ => planId
        };
    }

    private static string GetStatusDisplayName(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return "Free";

        return status.ToLowerInvariant() switch
        {
            "premium" => "Premium",
            "cancelled" => "Đã huỷ",
            "free" => "Free",
            _ => status
        };
    }

    private async void OnCancelSubscriptionClicked(object sender, EventArgs e)
    {
        if (!IsPremium)
        {
            return;
        }

        var confirm = await DisplayAlertAsync("Huỷ gói", "Bạn muốn huỷ gói Premium hiện tại?", "Huỷ gói", "Giữ lại");
        if (!confirm)
        {
            return;
        }

        var ok = await _apiService.CancelSubscriptionAsync(_userId);
        if (!ok)
        {
            await DisplayAlertAsync("Lỗi", "Không huỷ được gói đăng ký.", "OK");
            return;
        }

        await LoadDataAsync();
        IsOrderVisible = false;
        CurrentOrder = null;
        OrderInfoText = string.Empty;
        OnPropertyChanged(nameof(CurrentOrder));
        OnPropertyChanged(nameof(OrderInfoText));
        await DisplayAlertAsync("Thành công", "Đã huỷ gói Premium.", "OK");
    }

    private static void EnsureQrImageUrl(PaymentOrder order)
    {
        if (order == null)
        {
            return;
        }

        if (LooksLikeImageUrl(order.QrImageUrl))
        {
            return;
        }

        var dataForQr = !string.IsNullOrWhiteSpace(order.PaymentUrl)
            ? order.PaymentUrl
            : order.QrImageUrl;

        if (string.IsNullOrWhiteSpace(dataForQr))
        {
            return;
        }

        order.QrImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=320x320&data={Uri.EscapeDataString(dataForQr)}";
    }

    private static bool LooksLikeImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return false;
        }

        return url.Contains("create-qr-code", StringComparison.OrdinalIgnoreCase)
               || url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
               || url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
               || url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
               || url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
    }
}
