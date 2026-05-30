using CommunityToolkit.Mvvm.ComponentModel;
using KrishiAI.App.Services;

namespace KrishiAI.App.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public BaseViewModel()
    {
        // Subscribe to language changes
        Helpers.LocalizationManager.Instance.PropertyChanged += (s, e) =>
        {
            // When language changes, call OnLanguageChanged
            OnLanguageChanged();
        };
    }

    public virtual void OnAppearing() { }
    public virtual void OnDisappearing() { }

    // Override this in child ViewModels to refresh localized strings
    protected virtual void OnLanguageChanged() { }
}
