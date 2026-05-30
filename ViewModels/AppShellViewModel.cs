using CommunityToolkit.Mvvm.ComponentModel;

namespace KrishiAI.App.ViewModels;

public partial class AppShellViewModel : BaseViewModel
{
    [ObservableProperty]
    private string homeTitle = Resources.Strings.AppStrings.Home;

    [ObservableProperty]
    private string diseaseTitle = "Disease";

    [ObservableProperty]
    private string voiceTitle = "Voice";

    [ObservableProperty]
    private string historyTitle = Resources.Strings.AppStrings.History;

    [ObservableProperty]
    private string settingsTitle = Resources.Strings.AppStrings.Settings;

    public AppShellViewModel()
    {
        UpdateLocalizedStrings();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        UpdateLocalizedStrings();
        System.Diagnostics.Debug.WriteLine("🌍 AppShellViewModel: Language changed, updating tab titles");
    }

    private void UpdateLocalizedStrings()
    {
        HomeTitle = Resources.Strings.AppStrings.Home;
        DiseaseTitle = GetDiseaseTitle();
        VoiceTitle = GetVoiceTitle();
        HistoryTitle = Resources.Strings.AppStrings.History;
        SettingsTitle = Resources.Strings.AppStrings.Settings;
    }

    private string GetDiseaseTitle()
    {
        // Shortened versions for tab bar
        var culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
        return culture switch
        {
            "hi-IN" => "रोग",
            "mr-IN" => "रोग",
            "ta-IN" => "நோய்",
            "te-IN" => "వ్యాధి",
            "pa-IN" => "ਰੋਗ",
            "gu-IN" => "રોગ",
            "bn-IN" => "রোগ",
            _ => "Disease"
        };
    }

    private string GetVoiceTitle()
    {
        var culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
        return culture switch
        {
            "hi-IN" => "आवाज",
            "mr-IN" => "आवाज",
            "ta-IN" => "குரல்",
            "te-IN" => "వాయిస్",
            "pa-IN" => "ਆਵਾਜ਼",
            "gu-IN" => "વૉઇસ",
            "bn-IN" => "ভয়েস",
            _ => "Voice"
        };
    }
}
