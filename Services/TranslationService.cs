using System.Text;
using System.Text.Json;
using KrishiAI.App.Resources.Strings;

namespace KrishiAI.App.Services;

public class TranslationService : ITranslationService
{
    private readonly IAIChatService _aiService;
    private readonly IConfigurationService _config;
    private readonly string _cachePath;

    public TranslationService(IAIChatService aiService, IConfigurationService config)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        var folder = FileSystem.CacheDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _cachePath = Path.Combine(folder, "translations_cache.json");
    }

    public async Task<Dictionary<string, Dictionary<string, string>>?> GenerateMissingTranslationsAsync(Dictionary<string, string> englishSource, string targetLanguageCode)
    {
        if (englishSource == null || englishSource.Count == 0) return null;

        try
        {
            var cache = await ReadCacheAsync() ?? new Dictionary<string, Dictionary<string, string>>();

            var toTranslate = new Dictionary<string, string>();
            foreach (var kv in englishSource)
            {
                if (cache.TryGetValue(kv.Key, out var existing) && existing.ContainsKey(targetLanguageCode))
                    continue;

                toTranslate[kv.Key] = kv.Value;
            }

            if (toTranslate.Count == 0)
                return null; // nothing to translate

            var prompt = BuildTranslationPrompt(toTranslate, targetLanguageCode);
            var aiResponse = await _aiService.ProcessQueryAsync(prompt, targetLanguageCode);

            // Try to extract JSON object from response
            var json = ExtractJson(aiResponse);

            Dictionary<string, string>? translated = null;
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    translated = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                }
                catch
                {
                    translated = null;
                }
            }

            // Fallback: if parsing failed, map each key to the raw response (least useful)
            if (translated == null)
            {
                translated = new Dictionary<string, string>();
                foreach (var k in toTranslate.Keys)
                    translated[k] = aiResponse;
            }

            var additions = new Dictionary<string, Dictionary<string, string>>();
            foreach (var kv in translated)
            {
                additions[kv.Key] = new Dictionary<string, string> { [targetLanguageCode] = kv.Value };

                if (!cache.ContainsKey(kv.Key)) cache[kv.Key] = new Dictionary<string, string>();
                cache[kv.Key][targetLanguageCode] = kv.Value;
            }

            await WriteCacheAsync(cache);

            return additions;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TranslationService.GenerateMissingTranslationsAsync error: {ex.Message}");
            return null;
        }
    }

    private static string BuildTranslationPrompt(Dictionary<string, string> items, string targetLang)
    {
        // Ask the AI to return a pure JSON object mapping keys to translated strings
        var sb = new StringBuilder();
        sb.AppendLine($"Translate the following key/value pairs into {targetLang}. Return only a JSON object where keys are identical and values are the translated text. Do not add any other commentary.");
        sb.AppendLine("Input:");
        sb.AppendLine("{");
        var first = true;
        foreach (var kv in items)
        {
            if (!first) sb.AppendLine(",");
            first = false;
            var cleaned = kv.Value.Replace("\r", " ").Replace("\n", " ").Replace("\"", "'");
            sb.Append($"  \"{kv.Key}\": \"{cleaned}\"");
        }
        sb.AppendLine();
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string? ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var first = text.IndexOf('{');
        var last = text.LastIndexOf('}');
        if (first >= 0 && last > first)
        {
            return text.Substring(first, last - first + 1);
        }
        return null;
    }

    private async Task<Dictionary<string, Dictionary<string, string>>?> ReadCacheAsync()
    {
        try
        {
            if (!File.Exists(_cachePath)) return null;
            var txt = await File.ReadAllTextAsync(_cachePath);
            if (string.IsNullOrWhiteSpace(txt)) return null;
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(txt);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Translation cache read error: {ex.Message}");
            return null;
        }
    }

    private async Task WriteCacheAsync(Dictionary<string, Dictionary<string, string>> cache)
    {
        try
        {
            var txt = JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_cachePath, txt);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Translation cache write error: {ex.Message}");
        }
    }
}
