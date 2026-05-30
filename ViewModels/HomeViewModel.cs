using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Resources.Strings;

namespace KrishiAI.App.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    [ObservableProperty]
    private int notificationCount = 3;

    [ObservableProperty]
    private bool hasNotifications = true;

    [ObservableProperty]
    private string greeting = string.Empty;

    // Localized strings
    [ObservableProperty]
    private string farmer = AppStrings.Farmer;

    [ObservableProperty]
    private string krishiAI = AppStrings.KrishiAI;

    [ObservableProperty]
    private string yourAIFarmingCompanion = AppStrings.YourAIFarmingCompanion;

    [ObservableProperty]
    private string smartFarming = AppStrings.SmartFarming;

    [ObservableProperty]
    private string betterTomorrow = AppStrings.BetterTomorrow;

    [ObservableProperty]
    private string howCanIHelp = AppStrings.HowCanIHelp;

    [ObservableProperty]
    private string cropDiseaseDetection = AppStrings.CropDiseaseDetection;

    [ObservableProperty]
    private string voiceAssistant = AppStrings.VoiceAssistant;

    [ObservableProperty]
    private string quickFeatures = AppStrings.QuickFeatures;

    [ObservableProperty]
    private string languages = AppStrings.Languages;

    [ObservableProperty]
    private string history = AppStrings.History;

    [ObservableProperty]
    private string farmingTips = AppStrings.FarmingTips;

    [ObservableProperty]
    private string detectDisease = AppStrings.DetectDisease;

    [ObservableProperty]
    private string askQuestion = AppStrings.AskQuestion;

    [ObservableProperty]
    private string cropDiseaseDescriptionText = AppStrings.CropDiseaseDescription;

    [ObservableProperty]
    private string voiceAssistantDescriptionText = AppStrings.VoiceAssistantDescription;

    public HomeViewModel()
    {
        Title = "KrishiAI - Farmer Assistant";
        UpdateGreeting();
    }

    private void UpdateGreeting()
    {
        var hour = DateTime.Now.Hour;

        if (hour >= 5 && hour < 12)
        {
            Greeting = AppStrings.GoodMorning;
        }
        else if (hour >= 12 && hour < 17)
        {
            Greeting = AppStrings.GoodAfternoon;
        }
        else if (hour >= 17 && hour < 21)
        {
            Greeting = AppStrings.GoodEvening;
        }
        else
        {
            Greeting = AppStrings.GoodNight;
        }
    }

    private void UpdateLocalizedStrings()
    {
        Farmer = AppStrings.Farmer;
        KrishiAI = AppStrings.KrishiAI;
        YourAIFarmingCompanion = AppStrings.YourAIFarmingCompanion;
        SmartFarming = AppStrings.SmartFarming;
        BetterTomorrow = AppStrings.BetterTomorrow;
        HowCanIHelp = AppStrings.HowCanIHelp;
        CropDiseaseDetection = AppStrings.CropDiseaseDetection;
        VoiceAssistant = AppStrings.VoiceAssistant;
        QuickFeatures = AppStrings.QuickFeatures;
        Languages = AppStrings.Languages;
        History = AppStrings.History;
        FarmingTips = AppStrings.FarmingTips;
        DetectDisease = AppStrings.DetectDisease;
        AskQuestion = AppStrings.AskQuestion;
        CropDiseaseDescriptionText = AppStrings.CropDiseaseDescription;
        VoiceAssistantDescriptionText = AppStrings.VoiceAssistantDescription;
        UpdateGreeting(); // Refresh greeting in current language
    }

    public override void OnAppearing()
    {
        base.OnAppearing();
        UpdateGreeting(); // Refresh greeting when page appears
        UpdateLocalizedStrings(); // Refresh translations
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        UpdateLocalizedStrings(); // Refresh all strings when language changes
        System.Diagnostics.Debug.WriteLine("🌍 HomeViewModel: Language changed, updating strings");
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

    [RelayCommand]
    private async Task NavigateToNotifications()
    {
        try
        {
            await Shell.Current.GoToAsync("notifications");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToFarmerTips()
    {
        try
        {
            await Shell.Current.GoToAsync("farmertips");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToLanguages()
    {
        try
        {
            await Shell.Current.GoToAsync("languageselector");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToHistory()
    {
        await Shell.Current.GoToAsync("//history");
    }
}
