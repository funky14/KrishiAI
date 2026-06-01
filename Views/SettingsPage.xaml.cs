using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class SettingsPage : ContentPage
{
#pragma warning disable CA1416
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    // Parameterless constructor for Shell DataTemplate
    public SettingsPage()
    {
        var viewModel = Application.Current?.Handler?.MauiContext?.Services?.GetService<SettingsViewModel>();
        if (viewModel == null) throw new InvalidOperationException("Could not resolve SettingsViewModel");
        InitializeComponent();
        BindingContext = viewModel;
    }
#pragma warning restore CA1416
}
