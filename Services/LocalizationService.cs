using KrishiAI.App.Models;
using System.Globalization;

namespace KrishiAI.App.Services;

public class LocalizationService : ILocalizationService
{
    private readonly List<SupportedLanguage> _supportedLanguages;

    public event EventHandler<string>? LanguageChanged;

    public LocalizationService()
    {
        _supportedLanguages = new List<SupportedLanguage>
        {
            new SupportedLanguage { LanguageCode = "en-US", LanguageName = "English", NativeName = "English", VoiceName = "en-US-JennyNeural" },
            new SupportedLanguage { LanguageCode = "hi-IN", LanguageName = "Hindi", NativeName = "हिंदी", VoiceName = "hi-IN-SwaraNeural" },
            new SupportedLanguage { LanguageCode = "mr-IN", LanguageName = "Marathi", NativeName = "मराठी", VoiceName = "mr-IN-AarohiNeural" },
            new SupportedLanguage { LanguageCode = "ta-IN", LanguageName = "Tamil", NativeName = "தமிழ்", VoiceName = "ta-IN-PallaviNeural" },
            new SupportedLanguage { LanguageCode = "te-IN", LanguageName = "Telugu", NativeName = "తెలుగు", VoiceName = "te-IN-ShrutiNeural" },
            new SupportedLanguage { LanguageCode = "pa-IN", LanguageName = "Punjabi", NativeName = "ਪੰਜਾਬੀ", VoiceName = "pa-IN-GauriNeural" },
            new SupportedLanguage { LanguageCode = "gu-IN", LanguageName = "Gujarati", NativeName = "ગુજરાતી", VoiceName = "gu-IN-DhwaniNeural" },
            new SupportedLanguage { LanguageCode = "bn-IN", LanguageName = "Bengali", NativeName = "বাংলা", VoiceName = "bn-IN-TanishaaNeural" }
        };

        // Load saved language preference on initialization
        var savedLanguage = Preferences.Get("SelectedLanguage", "en-US");
        SetCulture(savedLanguage);
    }

    public List<SupportedLanguage> GetSupportedLanguages()
    {
        return _supportedLanguages;
    }

    public void SetCulture(string languageCode)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🌍 SetCulture called with: {languageCode}");

            var culture = new CultureInfo(languageCode);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Save preference
            Preferences.Set("SelectedLanguage", languageCode);

            System.Diagnostics.Debug.WriteLine($"🌍 Culture set, now notifying subscribers...");

            // Notify subscribers about language change
            LanguageChanged?.Invoke(this, languageCode);

            // Notify localization manager to update all bindings (on main thread)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                System.Diagnostics.Debug.WriteLine($"🌍 Notifying LocalizationManager...");
                Helpers.LocalizationManager.Instance.NotifyLanguageChanged();
                System.Diagnostics.Debug.WriteLine($"🌍 LocalizationManager notified!");
            });

            System.Diagnostics.Debug.WriteLine($"✅ Language changed to: {languageCode}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error setting culture: {ex.Message}");
            // Fallback to English
            var fallbackCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = fallbackCulture;
            CultureInfo.CurrentUICulture = fallbackCulture;
        }
    }

    public string GetCurrentLanguageCode()
    {
        return CultureInfo.CurrentUICulture.Name;
    }

    public SupportedLanguage GetCurrentLanguage()
    {
        var currentCode = GetCurrentLanguageCode();
        return _supportedLanguages.FirstOrDefault(l => l.LanguageCode == currentCode) 
               ?? _supportedLanguages.First();
    }
}
