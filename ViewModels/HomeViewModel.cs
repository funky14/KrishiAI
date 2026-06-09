using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Services;
using KrishiAI.App.Resources.Strings;

namespace KrishiAI.App.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private string greetingText = string.Empty;

    [ObservableProperty]
    private string plantHealthMonitorText = string.Empty;

    [ObservableProperty]
    private string plantHealthMonitorDescText = string.Empty;

    [ObservableProperty]
    private string howCanIHelpYouText = string.Empty;

    [ObservableProperty]
    private string cropDiseaseDetectionTitleText = string.Empty;

    [ObservableProperty]
    private string cropDiseaseDetectionDescText = string.Empty;

    [ObservableProperty]
    private string detectDiseaseText = string.Empty;

    [ObservableProperty]
    private string voiceAssistantTitleText = string.Empty;

    [ObservableProperty]
    private string voiceAssistantDescText = string.Empty;

    [ObservableProperty]
    private string askAQuestionText = string.Empty;

    [ObservableProperty]
    private string quickFeaturesText = string.Empty;

    [ObservableProperty]
    private string languagesText = string.Empty;

    [ObservableProperty]
    private string historyText = string.Empty;

    [ObservableProperty]
    private string farmingTipsText = string.Empty;

    [ObservableProperty]
    private string currentUserName = string.Empty;

    [ObservableProperty]
    private string currentUserPhone = string.Empty;

    public HomeViewModel(ILocalizationService localizationService, IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        InitializeLocalization(localizationService);
        Title = "KrishiAI - Farmer Assistant";
        UpdateLocalizedStrings();
        LoadCurrentUserInfo();
    }

    private void UpdateLocalizedStrings()
    {
        GreetingText = GetTimeBasedGreeting();
        PlantHealthMonitorText = AppStrings.PlantHealthMonitor;
        PlantHealthMonitorDescText = AppStrings.PlantHealthMonitorDesc;
        HowCanIHelpYouText = AppStrings.HowCanIHelpYou;
        CropDiseaseDetectionTitleText = AppStrings.CropDiseaseDetectionTitle;
        CropDiseaseDetectionDescText = AppStrings.CropDiseaseDetectionDesc;
        DetectDiseaseText = AppStrings.DetectDisease;
        VoiceAssistantTitleText = AppStrings.VoiceAssistantTitle;
        VoiceAssistantDescText = AppStrings.VoiceAssistantDesc;
        AskAQuestionText = AppStrings.AskAQuestion;
        QuickFeaturesText = AppStrings.QuickFeatures;
        LanguagesText = AppStrings.Languages;
        HistoryText = AppStrings.History;
        FarmingTipsText = AppStrings.FarmingTips;
    }

    public override void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
    }

    private string GetTimeBasedGreeting()
    {
        var hour = DateTime.Now.Hour;

        if (hour >= 5 && hour < 12)
            return AppStrings.GoodMorning;
        else if (hour >= 12 && hour < 17)
            return AppStrings.GoodAfternoon;
        else if (hour >= 17 && hour < 21)
            return AppStrings.GoodEvening;
        else
            return AppStrings.GoodNight;
    }

    private void LoadCurrentUserInfo()
    {
        try
        {
            var currentUser = _authenticationService.GetCurrentUserAsync().GetAwaiter().GetResult();
            if (currentUser != null)
            {
                CurrentUserName = currentUser.FullName ?? "User";
                CurrentUserPhone = currentUser.PhoneNumber ?? "No phone number";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading user info: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToDiseaseDetection()
    {
        await Shell.Current.GoToAsync("//disease");
    }

    [RelayCommand]
    private async Task NavigateToVoiceAssistant()
    {
        await Shell.Current.GoToAsync("//voice");
    }
}
