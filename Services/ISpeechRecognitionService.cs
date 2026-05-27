using KrishiAI.App.Models;

namespace KrishiAI.App.Services;

public interface ISpeechRecognitionService
{
    Task<string?> StartListeningAsync(string languageCode);
    Task StopListeningAsync();
    List<SupportedLanguage> GetSupportedLanguages();
}
