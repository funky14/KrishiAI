namespace KrishiAI.App.Models;

public class SupportedLanguage
{
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string VoiceName { get; set; } = string.Empty;
    public bool IsRTL { get; set; } = false;
    public string FlagIcon { get; set; } = string.Empty;
}
