using KrishiAI.App.Models;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class SpeechRecognitionService : ISpeechRecognitionService
{
    public async Task<string?> StartListeningAsync(string languageCode)
    {
        try
        {
            // Check microphone permission
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
                if (status != PermissionStatus.Granted)
                {
                    return null;
                }
            }

            // In production, integrate Azure Speech SDK or platform-specific APIs
            // For demo, simulate speech recognition
            await Task.Delay(2000); // Simulate listening

            // Mock transcription
            return GetMockTranscription(languageCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"StartListeningAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task StopListeningAsync()
    {
        await Task.CompletedTask;
    }

    public List<SupportedLanguage> GetSupportedLanguages()
    {
        return new List<SupportedLanguage>
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
    }

    private string GetMockTranscription(string languageCode)
    {
        var mockPhrases = new Dictionary<string, string>
        {
            ["en-US"] = "My tomato plants have yellow leaves",
            ["hi-IN"] = "मेरे टमाटर के पौधों की पत्तियां पीली हो रही हैं",
            ["mr-IN"] = "माझ्या टोमॅटोच्या झाडांची पाने पिवळी होत आहेत",
            ["ta-IN"] = "என் தக்காளி செடிகளின் இலைகள் மஞ்சள் நிறமாக உள்ளன",
            ["te-IN"] = "నా టమోటా మొక్కల ఆకులు పసుపు రంగులో ఉన్నాయి"
        };

        return mockPhrases.TryGetValue(languageCode, out var phrase) ? phrase : mockPhrases["en-US"];
    }
}
