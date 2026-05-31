using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    // Parameterless constructor for Shell DataTemplate
    public SettingsPage() : this(IPlatformApplication.Current!.Services.GetService<SettingsViewModel>()!)
    {
    }
}
