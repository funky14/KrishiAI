using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using KrishiAI.App.Resources.Strings;
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
    private ObservableCollection<VoiceCommand> conversationHistory = new();

    public string CurrentLanguageName => _localizationService.GetCurrentLanguage();


    [ObservableProperty]
    private string voiceAssistantText = string.Empty;

    [ObservableProperty]
    private string selectLanguageText = string.Empty;

    [ObservableProperty]
    private string tapToSpeakText = string.Empty;

    [ObservableProperty]
    private string noteSpeakText = string.Empty;

    [ObservableProperty]
    private string askVoiceAssistantText = string.Empty;

    [ObservableProperty]
    private string recordingText = string.Empty;

    [ObservableProperty]
    private string processingText = string.Empty;

    [ObservableProperty]
    private string transcriptionText = string.Empty;

    [ObservableProperty]
    private string aiResponseText = string.Empty;

    [ObservableProperty]
    private string conversationHistoryText = string.Empty;


    private readonly ILocalizationService _localizationService;

#pragma warning disable CS8618
    public VoiceAssistantViewModel(
        ISpeechRecognitionService speechService,
        ITextToSpeechService ttsService,
        IAIChatService chatService,
        ILocalizationService localizationService)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎤 Initializing VoiceAssistantViewModel...");

            _speechService = speechService;
            _ttsService = ttsService;
            _chatService = chatService;
            _localizationService = localizationService;

            InitializeLocalization(localizationService);

            Title = "Voice Assistant";

            UpdateLocalizedStrings();

            System.Diagnostics.Debug.WriteLine("✅ VoiceAssistantViewModel initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ VoiceAssistantViewModel initialization error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack trace: {ex.StackTrace}");

            // Ensure minimum initialization
            Title = "Voice Assistant";
        }
#pragma warning restore CS8618
    }

    private void UpdateLocalizedStrings()
    {
        VoiceAssistantText = AppStrings.VoiceAssistant;
        SelectLanguageText = AppStrings.SelectLanguage;
        TapToSpeakText = AppStrings.TapToSpeak;
        NoteSpeakText = AppStrings.NoteSpeak;
        AskVoiceAssistantText = AppStrings.AskVoiceAssistant;
        RecordingText = AppStrings.Recording;
        ProcessingText = AppStrings.Processing;
        TranscriptionText = AppStrings.Transcription;
        AiResponseText = AppStrings.AIResponse;
        ConversationHistoryText = AppStrings.ConversationHistory;
    }

    public override void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        OnPropertyChanged(nameof(CurrentLanguageName));
    }



    [RelayCommand]
    private async Task StartRecording()
    {
        try
        {
            IsRecording = true;
            CurrentTranscription = "Listening...";

            var languageCode = _localizationService.GetCurrentLanguageCode();
            var transcription = await _speechService.StartListeningAsync(languageCode);
            
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
            var languageCode = _localizationService.GetCurrentLanguageCode();

            // Add user message to history
            var userCommand = new VoiceCommand
            {
                CommandText = query,
                Language = languageCode,
                Timestamp = DateTime.Now,
                IsUserMessage = true
            };
            ConversationHistory.Add(userCommand);

            // Get AI response
            var response = await _chatService.ProcessQueryAsync(query, languageCode);

            // Add AI response to history
            var aiCommand = new VoiceCommand
            {
                CommandText = response,
                Language = languageCode,
                Timestamp = DateTime.Now,
                IsUserMessage = false
            };
            ConversationHistory.Add(aiCommand);

            // Speak response
            await _ttsService.SpeakAsync(response, languageCode);

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

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}
