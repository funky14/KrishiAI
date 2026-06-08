using KrishiAI.App.Models;
using KrishiAI.App.Resources.Strings;
using System.Globalization;

namespace KrishiAI.App.Services;

public class LocalizationService : ILocalizationService
{
    private List<SupportedLanguage> _supportedLanguages;

    public event EventHandler? LanguageChanged;

    public LocalizationService()
    {
        _supportedLanguages = new List<SupportedLanguage>
        {
            new SupportedLanguage 
            { 
                LanguageCode = "en-US", 
                LanguageName = "English", 
                NativeName = "English",
                VoiceName = "en-US-JennyNeural",
                IsRTL = false,
                FlagIcon = "🇬🇧"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "kn-IN", 
                LanguageName = "Kannada", 
                NativeName = "ಕನ್ನಡ",
                VoiceName = "kn-IN-GaganNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "hi-IN", 
                LanguageName = "Hindi", 
                NativeName = "हिंदी",
                VoiceName = "hi-IN-SwaraNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "mr-IN", 
                LanguageName = "Marathi", 
                NativeName = "मराठी",
                VoiceName = "mr-IN-AarohiNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "ta-IN", 
                LanguageName = "Tamil", 
                NativeName = "தமிழ்",
                VoiceName = "ta-IN-PallaviNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "te-IN", 
                LanguageName = "Telugu", 
                NativeName = "తెలుగు",
                VoiceName = "te-IN-ShrutiNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "pa-IN", 
                LanguageName = "Punjabi", 
                NativeName = "ਪੰਜਾਬੀ",
                VoiceName = "pa-IN-GianNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "gu-IN", 
                LanguageName = "Gujarati", 
                NativeName = "ગુજરાતી",
                VoiceName = "gu-IN-DhwaniNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "ml-IN", 
                LanguageName = "Malayalam", 
                NativeName = "മലയാളം",
                VoiceName = "ml-IN-SobhanaNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            },
            new SupportedLanguage 
            { 
                LanguageCode = "bn-IN", 
                LanguageName = "Bengali", 
                NativeName = "বাংলা",
                VoiceName = "bn-IN-BashkarNeural",
                IsRTL = false,
                FlagIcon = "🇮🇳"
            }
        };
    }

    public List<SupportedLanguage> GetSupportedLanguages()
    {
        return _supportedLanguages;
    }

    public void SetCulture(string languageCode)
    {
        try
        {
            var culture = new CultureInfo(languageCode);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;

            // Save preference
            Preferences.Set("AppLanguage", languageCode);

            // Notify listeners
            LanguageChanged?.Invoke(this, EventArgs.Empty);

            // Start background translation generation for any missing translations
            // (fire-and-forget)
            _ = GenerateMissingTranslationsAsync(languageCode);

            System.Diagnostics.Debug.WriteLine($"✅ Language changed to: {languageCode}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error setting culture: {ex.Message}");
        }
    }

    private async Task GenerateMissingTranslationsAsync(string languageCode)
    {
        try
        {
            var translator = MauiApplication.Current.Services.GetService(typeof(ITranslationService)) as ITranslationService;
            if (translator == null) return;

            // Build English source map from AppStrings static dictionary
            var english = new Dictionary<string, string>();
            // We will include a subset of commonly used keys to avoid large API usage.
            var keys = new[] { "Home", "WeatherAndIrrigation", "CurrentWeather", "WeatherRisks", "SevenDayForecast", "ViewAll", "Recommended", "ViewIrrigationPlan" };
            foreach (var k in keys)
            {
                english[k] = AppStrings.GetString(k) ?? k;
            }

            var additions = await translator.GenerateMissingTranslationsAsync(english, languageCode);
            if (additions != null)
            {
                // Register into AppStrings runtime
                AppStrings.RegisterTranslations(additions);
                // notify again so UI picks up new translations
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateMissingTranslationsAsync error: {ex.Message}");
        }
    }

    public string GetCurrentLanguageCode()
    {
        return CultureInfo.CurrentUICulture.Name;
    }

    public string GetCurrentLanguage()
    {
        var currentCode = GetCurrentLanguageCode();
        var language = _supportedLanguages.FirstOrDefault(l => l.LanguageCode == currentCode);
        return language?.LanguageName ?? "English";
    }
}
