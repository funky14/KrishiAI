using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class LanguageSelectorViewModel : BaseViewModel
{
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<SupportedLanguage> languages = new();

    [ObservableProperty]
    private SupportedLanguage? selectedLanguage;

    public LanguageSelectorViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        Title = Resources.Strings.AppStrings.ChooseLanguage;
        LoadLanguages();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        Title = Resources.Strings.AppStrings.ChooseLanguage;
        System.Diagnostics.Debug.WriteLine("🌍 LanguageSelectorViewModel: Language changed");
    }

    private void LoadLanguages()
    {
        // Get languages from the service
        var supportedLanguages = _localizationService.GetSupportedLanguages();
        Languages = new ObservableCollection<SupportedLanguage>(supportedLanguages);

        var currentLangCode = _localizationService.GetCurrentLanguageCode();
        SelectedLanguage = Languages.FirstOrDefault(l => l.LanguageCode == currentLangCode);
    }

    [RelayCommand]
    private async Task SelectLanguage(SupportedLanguage language)
    {
        if (language == null) return;

        SelectedLanguage = language;
        _localizationService.SetCulture(language.LanguageCode);

        await Shell.Current.DisplayAlert("Success", $"Language changed to {language.NativeName}", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.GoToAsync("..");
    }
}
