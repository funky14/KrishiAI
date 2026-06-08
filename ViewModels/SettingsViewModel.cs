using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using KrishiAI.App.Resources.Strings;
using KrishiAI.App.Views;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private readonly ILocalizationService _localizationService;
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private SupportedLanguage? selectedLanguage;

    [ObservableProperty]
    private bool saveHistory = true;

    [ObservableProperty]
    private bool autoPlayResponses = true;

    [ObservableProperty]
    private string appVersion = "1.0.0";

    [ObservableProperty]
    private string settingsText = string.Empty;

    [ObservableProperty]
    private string preferencesText = string.Empty;

    [ObservableProperty]
    private string defaultLanguageText = string.Empty;

    [ObservableProperty]
    private string saveHistoryText = string.Empty;

    [ObservableProperty]
    private string saveHistoryDescText = string.Empty;

    [ObservableProperty]
    private string autoPlayResponsesText = string.Empty;

    [ObservableProperty]
    private string autoPlayResponsesDescText = string.Empty;

    [ObservableProperty]
    private string dataManagementText = string.Empty;

    [ObservableProperty]
    private string clearCacheText = string.Empty;

    [ObservableProperty]
    private string clearDetectionHistoryText = string.Empty;

    [ObservableProperty]
    private string aboutKrishiAIText = string.Empty;

    [ObservableProperty]
    private string appNameText = string.Empty;

    [ObservableProperty]
    private string appDescriptionText = string.Empty;

    [ObservableProperty]
    private string featuresText = string.Empty;

    [ObservableProperty]
    private string featureAIPoweredText = string.Empty;

    [ObservableProperty]
    private string featureMultiLanguageText = string.Empty;

    [ObservableProperty]
    private string featureVoiceBasedText = string.Empty;

    [ObservableProperty]
    private string featureTreatmentText = string.Empty;

    [ObservableProperty]
    private string featureOfflineFirstText = string.Empty;

    [ObservableProperty]
    private string copyrightText = string.Empty;

    [ObservableProperty]
    private string saveSettingsText = string.Empty;

    [ObservableProperty]
    private string versionText = string.Empty;

    public ObservableCollection<SupportedLanguage> SupportedLanguages { get; set; }

    public SettingsViewModel(IDatabaseService databaseService, ILocalizationService localizationService, IAuthenticationService authenticationService)
    {
        _databaseService = databaseService;
        _localizationService = localizationService;
        _authenticationService = authenticationService;

        InitializeLocalization(localizationService);

        Title = "Settings";

        SupportedLanguages = new ObservableCollection<SupportedLanguage>(_localizationService.GetSupportedLanguages());

        LoadSettings();
        UpdateLocalizedStrings();
    }

    private void LoadSettings()
    {
        // Load settings from preferences
        var savedLanguageCode = Preferences.Get("AppLanguage", "en-US");
        SelectedLanguage = SupportedLanguages.FirstOrDefault(l => l.LanguageCode == savedLanguageCode) 
                          ?? SupportedLanguages.First();

        SaveHistory = Preferences.Get(nameof(SaveHistory), true);
        AutoPlayResponses = Preferences.Get(nameof(AutoPlayResponses), true);
    }

    private void UpdateLocalizedStrings()
    {
        SettingsText = AppStrings.Settings;
        PreferencesText = AppStrings.Preferences;
        DefaultLanguageText = AppStrings.DefaultLanguage;
        SaveHistoryText = AppStrings.SaveHistory;
        SaveHistoryDescText = AppStrings.SaveHistoryDesc;
        AutoPlayResponsesText = AppStrings.AutoPlayResponses;
        AutoPlayResponsesDescText = AppStrings.AutoPlayResponsesDesc;
        DataManagementText = AppStrings.DataManagement;
        ClearCacheText = AppStrings.ClearCache;
        ClearDetectionHistoryText = AppStrings.ClearDetectionHistory;
        AboutKrishiAIText = AppStrings.AboutKrishiAI;
        AppNameText = AppStrings.AppName;
        AppDescriptionText = AppStrings.AppDescription;
        FeaturesText = AppStrings.Features;
        FeatureAIPoweredText = AppStrings.FeatureAIPowered;
        FeatureMultiLanguageText = AppStrings.FeatureMultiLanguage;
        FeatureVoiceBasedText = AppStrings.FeatureVoiceBased;
        FeatureTreatmentText = AppStrings.FeatureTreatment;
        FeatureOfflineFirstText = AppStrings.FeatureOfflineFirst;
        CopyrightText = AppStrings.Copyright;
        SaveSettingsText = AppStrings.SaveSettings;
        VersionText = AppStrings.Version;
    }

    public override void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
    }

    [RelayCommand]
    private async Task SaveSettingsCommand()
    {
        if (SelectedLanguage != null)
        {
            Preferences.Set("AppLanguage", SelectedLanguage.LanguageCode);
            _localizationService.SetCulture(SelectedLanguage.LanguageCode);
        }

        Preferences.Set(nameof(SaveHistory), SaveHistory);
        Preferences.Set(nameof(AutoPlayResponses), AutoPlayResponses);

        await Application.Current!.MainPage!.DisplayAlert(
            AppStrings.Success, 
            AppStrings.SettingsSavedSuccessfully, 
            AppStrings.OK);
    }

    [RelayCommand]
    private async Task ClearCache()
    {
        try
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                AppStrings.ClearCache,
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

                await Application.Current!.MainPage!.DisplayAlert(
                    AppStrings.Success, 
                    AppStrings.CacheClearedSuccessfully, 
                    AppStrings.OK);
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
                AppStrings.ClearDetectionHistory,
                AppStrings.ClearHistoryConfirm,
                AppStrings.Yes,
                AppStrings.No);

            if (confirm)
            {
                await _databaseService.ClearHistoryAsync();
                await Application.Current!.MainPage!.DisplayAlert(
                    AppStrings.Success, 
                    AppStrings.HistoryClearedSuccessfully, 
                    AppStrings.OK);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error clearing history: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task Logout()
    {
        try
        {
            IsBusy = true;

            // Confirm logout
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Logout",
                "Are you sure you want to logout?",
                "Yes",
                "No");

            if (!confirm)
            {
                IsBusy = false;
                return;
            }

            // Call authentication service logout
            await _authenticationService.LogoutAsync();

            System.Diagnostics.Debug.WriteLine("✅ User logged out successfully");

            // Navigate to login page by setting MainPage
            var authViewModel = IPlatformApplication.Current?.Services.GetService<AuthViewModel>();
            if (authViewModel != null)
            {
                var loginPage = new LoginPage(authViewModel);
                Application.Current!.MainPage = new NavigationPage(loginPage)
                {
                    BarBackgroundColor = (Color)Application.Current.Resources["Primary"],
                    BarTextColor = Colors.White
                };
                System.Diagnostics.Debug.WriteLine("✅ Navigated to LoginPage");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ AuthViewModel not available");
                ErrorMessage = "Failed to navigate to login";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Logout Error: {ex.Message}");
            ErrorMessage = $"Error logging out: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                $"Failed to logout: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
