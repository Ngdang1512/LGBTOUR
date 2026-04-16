using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;

namespace SaigonAudioTour.Mobile;

public class Language
{
    public required string Code { get; set; }
    public required string DisplayName { get; set; }
    public required string FlagEmoji { get; set; }
}

public partial class SplashScreen : ContentPage, INotifyPropertyChanged
{
    private Language? _selectedLanguage;
    
    public ObservableCollection<Language> Languages { get; } = new();
    
    public Language? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage != value)
            {
                _selectedLanguage = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SelectLanguageCommand { get; }
    public ICommand ContinueCommand { get; }
    public ICommand SkipCommand { get; }

    public SplashScreen()
    {
        InitializeComponent();
        BindingContext = this;

        InitializeLanguages();
        SelectLanguageCommand = new Command<Language>(OnLanguageSelected);
        ContinueCommand = new Command(OnContinue);
        SkipCommand = new Command(OnSkip);
    }

    private void InitializeLanguages()
    {
        Languages.Add(new Language { Code = "vi", DisplayName = "Tiếng Việt", FlagEmoji = "🇻🇳" });
        Languages.Add(new Language { Code = "en", DisplayName = "English", FlagEmoji = "🇬🇧" });
        Languages.Add(new Language { Code = "zh", DisplayName = "中文", FlagEmoji = "🇨🇳" });
        Languages.Add(new Language { Code = "ja", DisplayName = "日本語", FlagEmoji = "🇯🇵" });
        Languages.Add(new Language { Code = "ko", DisplayName = "한국어", FlagEmoji = "🇰🇷" });
        Languages.Add(new Language { Code = "fr", DisplayName = "Français", FlagEmoji = "🇫🇷" });

        // Default to Vietnamese
        SelectedLanguage = Languages[0];
    }

    private void OnLanguageSelected(Language language)
    {
        SelectedLanguage = language;
    }

    private void OnLanguageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Language language)
        {
            OnLanguageSelected(language);
        }
    }

    private async void OnContinue()
    {
        if (SelectedLanguage == null)
        {
            await DisplayAlertAsync("Thông báo", "Vui lòng chọn ngôn ngữ", "OK");
            return;
        }

        // Save selected app language to preferences
        Services.AppLanguageService.SetAppLanguage(SelectedLanguage.Code);

        if (string.IsNullOrWhiteSpace(Preferences.Get(Services.StorageKeys.NarrationLanguage, string.Empty)))
        {
            Services.AppLanguageService.SetNarrationLanguage(SelectedLanguage.Code);
        }

        // Chuyển về MainPage theo root NavigationPage
        App.SetRootPage(new AppShell());
    }

    private async void OnSkip()
    {
        // Use default Vietnamese for the app language
        Services.AppLanguageService.SetAppLanguage("vi");

        if (string.IsNullOrWhiteSpace(Preferences.Get(Services.StorageKeys.NarrationLanguage, string.Empty)))
        {
            Services.AppLanguageService.SetNarrationLanguage("vi");
        }

        App.SetRootPage(new AppShell());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Optional: Add entrance animation
        await this.FadeToAsync(1, 300);
    }
}