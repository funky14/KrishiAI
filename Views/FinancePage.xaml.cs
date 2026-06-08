using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class FinancePage : ContentPage
{
    private readonly FinanceViewModel _viewModel;

    public FinancePage(FinanceViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadFinancialSummaryAsync();
        await _viewModel.LoadTransactionsAsync();
    }
}
