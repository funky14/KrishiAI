namespace KrishiAI.App.Services;

public interface ITranslationService
{
    /// <summary>
    /// Generate translations for the provided English source strings into the target language.
    /// Returns a dictionary mapping key -> (languageCode -> translatedText)
    /// </summary>
    Task<Dictionary<string, Dictionary<string, string>>?> GenerateMissingTranslationsAsync(Dictionary<string, string> englishSource, string targetLanguageCode);
}
