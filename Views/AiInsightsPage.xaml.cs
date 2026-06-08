using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class AiInsightsPage : ContentPage
{
    public AiInsightsPage(AiInsightsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AiInsightsViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
