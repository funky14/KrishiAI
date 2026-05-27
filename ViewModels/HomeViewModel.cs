using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KrishiAI.App.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    public HomeViewModel()
    {
        Title = "KrishiAI - Farmer Assistant";
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
}
