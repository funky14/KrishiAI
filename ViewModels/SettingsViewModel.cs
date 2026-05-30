using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using KrishiAI.App.Resources.Strings;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private SupportedLanguage? selectedLanguage;

    [ObservableProperty]
    private ObservableCollection<SupportedLanguage> availableLanguages = new();

    [ObservableProperty]
    private bool saveHistory = true;

    [ObservableProperty]
    private bool autoPlayResponses = true;

    [ObservableProperty]
    private string appVersion = "1.0.0";

    // Localized strings - these will be observable and update when language changes
    [ObservableProperty]
    private string preferencesText = AppStrings.Preferences;

    [ObservableProperty]
    private string defaultLanguageText = AppStrings.DefaultLanguage;

    [ObservableProperty]
    private string languageHelpText = AppStrings.LanguageHelpText;

    [ObservableProperty]
    private string saveHistoryText = AppStrings.SaveHistory;

    [ObservableProperty]
    private string saveHistoryDescText = AppStrings.SaveHistoryDescription;

    [ObservableProperty]
    private string autoPlayText = AppStrings.AutoPlayResponses;

    [ObservableProperty]
    private string autoPlayDescText = AppStrings.AutoPlayResponsesDescription;

    [ObservableProperty]
    private string saveSettingsText = AppStrings.SaveSettings;

    [ObservableProperty]
    private string dataManagementText = AppStrings.DataManagement;

    [ObservableProperty]
    private string clearCacheText = AppStrings.ClearCache;

    [ObservableProperty]
    private string clearHistoryText = AppStrings.ClearHistory;

    [ObservableProperty]
    private string aboutText = AppStrings.About;

    [ObservableProperty]
    private string appDescriptionText = AppStrings.AppDescription;

    [ObservableProperty]
    private string featuresTitleText = AppStrings.FeaturesTitle;

    [ObservableProperty]
    private string feature1Text = AppStrings.Feature1Text;

    [ObservableProperty]
    private string feature2Text = AppStrings.Feature2Text;

    [ObservableProperty]
    private string feature3Text = AppStrings.Feature3Text;

    [ObservableProperty]
    private string feature4Text = AppStrings.Feature4Text;

    [ObservableProperty]
    private string copyrightText = AppStrings.CopyrightText;

    [ObservableProperty]
    private string appNameText = AppStrings.KrishiAI;

    [ObservableProperty]
    private string appTaglineText = AppStrings.YourAIFarmingCompanion;

    public SettingsViewModel(IDatabaseService databaseService, ILocalizationService localizationService)
    {
        _databaseService = databaseService;
        _localizationService = localizationService;
        Title = AppStrings.Settings;
        LoadSettings();

        // Subscribe to language change event
        _localizationService.LanguageChanged += OnLanguageChanged;

        // Subscribe to SelectedLanguage property changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedLanguage) && SelectedLanguage != null)
            {
                // Immediately apply language change when user selects from picker
                System.Diagnostics.Debug.WriteLine($"🌍 SettingsViewModel: Language selected: {SelectedLanguage.NativeName}");
                _localizationService.SetCulture(SelectedLanguage.LanguageCode);
                Preferences.Set(nameof(SelectedLanguage), SelectedLanguage.LanguageCode);
            }
        };
    }

    private void OnLanguageChanged(object? sender, string newLanguageCode)
    {
        // Update all localized strings when language changes
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        Title = AppStrings.Settings;
        PreferencesText = AppStrings.Preferences;
        DefaultLanguageText = AppStrings.DefaultLanguage;
        LanguageHelpText = AppStrings.LanguageHelpText;
        SaveHistoryText = AppStrings.SaveHistory;
        SaveHistoryDescText = AppStrings.SaveHistoryDescription;
        AutoPlayText = AppStrings.AutoPlayResponses;
        AutoPlayDescText = AppStrings.AutoPlayResponsesDescription;
        SaveSettingsText = AppStrings.SaveSettings;
        DataManagementText = AppStrings.DataManagement;
        ClearCacheText = AppStrings.ClearCache;
        ClearHistoryText = AppStrings.ClearHistory;
        AboutText = AppStrings.About;
        AppDescriptionText = AppStrings.AppDescription;
        FeaturesTitleText = AppStrings.FeaturesTitle;
        Feature1Text = AppStrings.Feature1Text;
        Feature2Text = AppStrings.Feature2Text;
        Feature3Text = AppStrings.Feature3Text;
        Feature4Text = AppStrings.Feature4Text;
        CopyrightText = AppStrings.CopyrightText;
        AppNameText = AppStrings.KrishiAI;
        AppTaglineText = AppStrings.YourAIFarmingCompanion;
    }

    private void LoadSettings()
    {
        // Load available languages
        var languages = _localizationService.GetSupportedLanguages();
        AvailableLanguages = new ObservableCollection<SupportedLanguage>(languages);

        // Load current language
        var currentLanguageCode = Preferences.Get(nameof(SelectedLanguage), "en-US");
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.LanguageCode == currentLanguageCode)
                          ?? AvailableLanguages.First();

        // Load other settings
        SaveHistory = Preferences.Get(nameof(SaveHistory), true);
        AutoPlayResponses = Preferences.Get(nameof(AutoPlayResponses), true);
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            if (SelectedLanguage != null)
            {
                // Apply the language change to the entire app
                _localizationService.SetCulture(SelectedLanguage.LanguageCode);
                Preferences.Set(nameof(SelectedLanguage), SelectedLanguage.LanguageCode);
            }

            Preferences.Set(nameof(SaveHistory), SaveHistory);
            Preferences.Set(nameof(AutoPlayResponses), AutoPlayResponses);

            await Application.Current!.MainPage!.DisplayAlert(
                AppStrings.Success, 
                AppStrings.SettingsSavedSuccess, 
                AppStrings.OK);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving settings: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert(AppStrings.Error, ErrorMessage, AppStrings.OK);
        }
    }

    [RelayCommand]
    private async Task ClearCache()
    {
        try
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                AppStrings.ClearCache.Replace("🗑️ ", ""),
                AppStrings.ClearCacheConfirm,
                AppStrings.Yes,
                AppStrings.No);

            if (confirm)
            {
                var cacheDir = FileSystem.CacheDirectory;
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                    Directory.CreateDirectory(cacheDir);
                }

                await Application.Current!.MainPage!.DisplayAlert(AppStrings.Success, AppStrings.CacheClearedSuccess, AppStrings.OK);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error clearing cache: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ClearHistory()
    {
        try
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                AppStrings.ClearHistory.Replace("🗑️ ", ""),
                AppStrings.ClearHistoryConfirm,
                AppStrings.Yes,
                AppStrings.No);

            if (confirm)
            {
                await _databaseService.ClearHistoryAsync();
                await Application.Current!.MainPage!.DisplayAlert(AppStrings.Success, AppStrings.HistoryClearedSuccess, AppStrings.OK);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error clearing history: {ex.Message}";
        }
    }
}
