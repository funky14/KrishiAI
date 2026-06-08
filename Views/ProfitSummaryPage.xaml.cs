using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class ProfitSummaryPage : ContentPage
{
    public ProfitSummaryPage(ProfitSummaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfitSummaryViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
