using CommunityToolkit.Mvvm.ComponentModel;
using KrishiAI.App.Services;
using Microsoft.Maui.ApplicationModel;

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
        // Ensure language change handling runs on the UI thread because handlers update bound properties
        LocalizationService.LanguageChanged += (s, e) =>
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => OnLanguageChanged());
            }
            catch
            {
                // Fallback: invoke directly if MainThread isn't available (design-time)
                OnLanguageChanged();
            }
        };
    }
}
