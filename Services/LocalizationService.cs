using KrishiAI.App.Models;
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

            System.Diagnostics.Debug.WriteLine($"✅ Language changed to: {languageCode}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error setting culture: {ex.Message}");
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
