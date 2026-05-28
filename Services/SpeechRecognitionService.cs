using KrishiAI.App.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class SpeechRecognitionService : ISpeechRecognitionService
{
    private readonly IConfigurationService _configService;

    public SpeechRecognitionService(IConfigurationService configService)
    {
        _configService = configService;
    }

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

            var config = await _configService.GetConfigurationAsync();

            // Use real Azure Speech if configured
            if (config.UseRealSpeechRecognition && 
                !string.IsNullOrEmpty(config.SpeechServiceKey))
            {
                return await RecognizeWithAzureSpeechAsync(languageCode, config);
            }

            // Fallback to mock for testing
            Debug.WriteLine("⚠️ Using mock speech recognition (Azure Speech not configured)");
            await Task.Delay(2000); // Simulate listening
            return GetMockTranscription(languageCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"StartListeningAsync Error: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> RecognizeWithAzureSpeechAsync(string languageCode, AzureConfiguration config)
    {
        try
        {
            var speechConfig = SpeechConfig.FromSubscription(config.SpeechServiceKey, config.SpeechServiceRegion);
            speechConfig.SpeechRecognitionLanguage = languageCode;

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

            Debug.WriteLine($"🎤 Listening in {languageCode}...");
            var result = await recognizer.RecognizeOnceAsync();

            switch (result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Debug.WriteLine($"✅ Recognized: {result.Text}");
                    return result.Text;
                    
                case ResultReason.NoMatch:
                    Debug.WriteLine("⚠️ No speech could be recognized.");
                    return null;
                    
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(result);
                    Debug.WriteLine($"❌ Recognition canceled: {cancellation.Reason}");
                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Debug.WriteLine($"Error: {cancellation.ErrorDetails}");
                    }
                    return null;
                    
                default:
                    return null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RecognizeWithAzureSpeechAsync Error: {ex.Message}");
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
