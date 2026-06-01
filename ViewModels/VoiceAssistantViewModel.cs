using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class VoiceAssistantViewModel : BaseViewModel
{
    private readonly ISpeechRecognitionService _speechService;
    private readonly ITextToSpeechService _ttsService;
    private readonly IAIChatService _chatService;

    [ObservableProperty]
    private bool isRecording;

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private string currentTranscription = string.Empty;

    [ObservableProperty]
    private SupportedLanguage selectedLanguage;

    [ObservableProperty]
    private ObservableCollection<VoiceCommand> conversationHistory = new();

    public ObservableCollection<SupportedLanguage> SupportedLanguages { get; set; }

#pragma warning disable CS8618
    public VoiceAssistantViewModel(
        ISpeechRecognitionService speechService,
        ITextToSpeechService ttsService,
        IAIChatService chatService)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎤 Initializing VoiceAssistantViewModel...");
            
            _speechService = speechService;
            _ttsService = ttsService;
            _chatService = chatService;

            Title = "Voice Assistant";
            
            var languages = _speechService.GetSupportedLanguages();
            System.Diagnostics.Debug.WriteLine($"   Languages loaded: {languages?.Count ?? 0}");
            
            SupportedLanguages = new ObservableCollection<SupportedLanguage>(languages ?? new List<SupportedLanguage>());
            
            // Auto-detect device language
            if (languages != null && languages.Any())
            {
                SelectedLanguage = DetectDeviceLanguage(languages);
                System.Diagnostics.Debug.WriteLine($"   Selected language: {SelectedLanguage?.LanguageName ?? "None"}");
            }
            
            System.Diagnostics.Debug.WriteLine("✅ VoiceAssistantViewModel initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ VoiceAssistantViewModel initialization error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack trace: {ex.StackTrace}");
            
            // Ensure minimum initialization
            Title = "Voice Assistant";
            SupportedLanguages = new ObservableCollection<SupportedLanguage>();
        }
#pragma warning restore CS8618
    }

    private SupportedLanguage DetectDeviceLanguage(List<SupportedLanguage> languages)
    {
        try
        {
            // Get device's current culture/language
            var deviceLanguage = System.Globalization.CultureInfo.CurrentCulture.Name; // e.g., "en-US", "hi-IN"
            
            // Try exact match first
            var matchedLanguage = languages.FirstOrDefault(l => 
                l.LanguageCode.Equals(deviceLanguage, StringComparison.OrdinalIgnoreCase));
            
            if (matchedLanguage != null)
                return matchedLanguage;
            
            // Try partial match (e.g., "hi" matches "hi-IN")
            var languagePrefix = deviceLanguage.Split('-')[0]; // Get "hi" from "hi-IN"
            matchedLanguage = languages.FirstOrDefault(l => 
                l.LanguageCode.StartsWith(languagePrefix, StringComparison.OrdinalIgnoreCase));
            
            if (matchedLanguage != null)
                return matchedLanguage;
            
            // Default to English if no match
            return languages.FirstOrDefault(l => l.LanguageCode.StartsWith("en")) ?? languages.First();
        }
        catch
        {
            // Fallback to first language (English)
            return languages.First();
        }
    }

    [RelayCommand]
    private async Task StartRecording()
    {
        try
        {
            IsRecording = true;
            CurrentTranscription = "Listening...";

            var transcription = await _speechService.StartListeningAsync(SelectedLanguage.LanguageCode);
            
            if (!string.IsNullOrEmpty(transcription))
            {
                CurrentTranscription = transcription;
                await ProcessQuery(transcription);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Recording error: {ex.Message}";
        }
        finally
        {
            IsRecording = false;
        }
    }

    [RelayCommand]
    private async Task StopRecording()
    {
        await _speechService.StopListeningAsync();
        IsRecording = false;
    }

    private async Task ProcessQuery(string query)
    {
        try
        {
            IsProcessing = true;

            // Add user message to history
            var userCommand = new VoiceCommand
            {
                CommandText = query,
                Language = SelectedLanguage.LanguageCode,
                Timestamp = DateTime.Now,
                IsUserMessage = true
            };
            ConversationHistory.Add(userCommand);

            // Get AI response
            var response = await _chatService.ProcessQueryAsync(query, SelectedLanguage.LanguageCode);

            // Add AI response to history
            var aiCommand = new VoiceCommand
            {
                CommandText = response,
                Language = SelectedLanguage.LanguageCode,
                Timestamp = DateTime.Now,
                IsUserMessage = false
            };
            ConversationHistory.Add(aiCommand);

            // Speak response
            await _ttsService.SpeakAsync(response, SelectedLanguage.LanguageCode);

            CurrentTranscription = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Processing error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task PlayResponse(VoiceCommand command)
    {
        if (command != null && !command.IsUserMessage)
        {
            await _ttsService.SpeakAsync(command.CommandText, command.Language);
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        ConversationHistory.Clear();
        CurrentTranscription = string.Empty;
    }
}
