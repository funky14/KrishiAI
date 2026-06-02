namespace KrishiAI.App.Services;

public interface ILocalizationService
{
    event EventHandler? LanguageChanged;

    List<Models.SupportedLanguage> GetSupportedLanguages();
    void SetCulture(string languageCode);
    string GetCurrentLanguageCode();
    string GetCurrentLanguage();
}
