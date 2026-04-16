using System.Globalization;

namespace SaigonAudioTour.Mobile.Services;

public static class AppLanguageService
{
    public static event EventHandler<string>? LanguageChanged;

    public static string GetAppLanguage()
        => NormalizeLanguageCode(Preferences.Get(StorageKeys.AppLanguage, "vi"));

    public static string GetNarrationLanguage()
        => NormalizeLanguageCode(Preferences.Get(StorageKeys.NarrationLanguage, GetAppLanguage()));

    public static void SetAppLanguage(string languageCode)
    {
        var normalized = NormalizeLanguageCode(languageCode);
        Preferences.Set(StorageKeys.AppLanguage, normalized);
        ApplyCulture(normalized);
        LanguageChanged?.Invoke(null, normalized);
    }

    public static void SetNarrationLanguage(string languageCode)
    {
        Preferences.Set(StorageKeys.NarrationLanguage, NormalizeLanguageCode(languageCode));
    }

    public static void ApplyCurrentAppLanguage()
    {
        ApplyCulture(GetAppLanguage());
    }

    public static string ToDisplayName(string languageCode)
    {
        return NormalizeLanguageCode(languageCode) switch
        {
            "en" => "English",
            "zh" => "中文",
            "ja" => "日本語",
            "ko" => "한국어",
            "fr" => "Français",
            _ => "Tiếng Việt"
        };
    }

    public static string ToCultureName(string languageCode)
    {
        return NormalizeLanguageCode(languageCode) switch
        {
            "en" => "en-US",
            "zh" => "zh-CN",
            "ja" => "ja-JP",
            "ko" => "ko-KR",
            "fr" => "fr-FR",
            _ => "vi-VN"
        };
    }

    public static string NormalizeLanguageCode(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return "vi";
        }

        var normalized = languageCode.Trim().ToLowerInvariant();
        return normalized switch
        {
            "vi" or "en" or "zh" or "ja" or "ko" or "fr" => normalized,
            _ => "vi"
        };
    }

    private static void ApplyCulture(string languageCode)
    {
        var culture = CultureInfo.GetCultureInfo(ToCultureName(languageCode));
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
