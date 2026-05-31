using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    // Parameterless constructor for Shell DataTemplate
    public HomePage() : this(IPlatformApplication.Current!.Services.GetService<HomeViewModel>()!)
    {
    }
}
