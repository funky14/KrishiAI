using CommunityToolkit.Mvvm.ComponentModel;

namespace KrishiAI.App.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public virtual void OnAppearing() { }
    public virtual void OnDisappearing() { }
}
