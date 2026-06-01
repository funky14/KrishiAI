using CommunityToolkit.Mvvm.ComponentModel;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    protected ILocalizationService? LocalizationService { get; private set; }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public virtual void OnAppearing() { }
    public virtual void OnDisappearing() { }

    public virtual void OnLanguageChanged() { }

    protected void InitializeLocalization(ILocalizationService localizationService)
    {
        LocalizationService = localizationService;
        LocalizationService.LanguageChanged += (s, e) => OnLanguageChanged();
    }
}
