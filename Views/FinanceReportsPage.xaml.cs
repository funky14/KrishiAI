using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class FinanceReportsPage : ContentPage
{
    public FinanceReportsPage(FinanceReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FinanceReportsViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
