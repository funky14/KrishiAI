using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface ILocalizationService
{
    event EventHandler<string>? LanguageChanged;

    List<SupportedLanguage> GetSupportedLanguages();
    void SetCulture(string languageCode);
    string GetCurrentLanguageCode();
    SupportedLanguage GetCurrentLanguage();
}
