using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private string selectedLanguage = "en-US";

    [ObservableProperty]
    private bool saveHistory = true;

    [ObservableProperty]
    private bool autoPlayResponses = true;

    [ObservableProperty]
    private string appVersion = "1.0.0";

    public SettingsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Settings";
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Load settings from preferences
        SelectedLanguage = Preferences.Get(nameof(SelectedLanguage), "en-US");
        SaveHistory = Preferences.Get(nameof(SaveHistory), true);
        AutoPlayResponses = Preferences.Get(nameof(AutoPlayResponses), true);
    }

    [RelayCommand]
    private void SaveSettings()
    {
        Preferences.Set(nameof(SelectedLanguage), SelectedLanguage);
        Preferences.Set(nameof(SaveHistory), SaveHistory);
        Preferences.Set(nameof(AutoPlayResponses), AutoPlayResponses);

        Application.Current!.MainPage!.DisplayAlert("Success", "Settings saved successfully", "OK");
    }

    [RelayCommand]
    private async Task ClearCache()
    {
        try
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Clear Cache",
                "This will delete all cached images. Continue?",
                "Yes",
                "No");

            if (confirm)
            {
                var cacheDir = FileSystem.CacheDirectory;
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                    Directory.CreateDirectory(cacheDir);
                }

                await Application.Current!.MainPage!.DisplayAlert("Success", "Cache cleared successfully", "OK");
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
                "Clear History",
                "This will delete all detection history. Continue?",
                "Yes",
                "No");

            if (confirm)
            {
                await _databaseService.ClearHistoryAsync();
                await Application.Current!.MainPage!.DisplayAlert("Success", "History cleared successfully", "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error clearing history: {ex.Message}";
        }
    }
}
