namespace KrishiAI.App.Models;

public class SupportedLanguage
{
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string VoiceName { get; set; } = string.Empty;
    public bool IsRTL { get; set; } = false;
    public string FlagIcon { get; set; } = string.Empty;

    /// <summary>
    /// Display name that combines native name with English name in brackets
    /// English only shows the language name without brackets
    /// </summary>
    public string DisplayName => LanguageCode == "en-US" || LanguageName == "English"
        ? LanguageName 
        : (string.IsNullOrEmpty(NativeName) ? LanguageName : $"{NativeName} ({LanguageName})");
}
