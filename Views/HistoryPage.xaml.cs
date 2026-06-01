using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class HistoryPage : ContentPage
{
#pragma warning disable CA1416
    private readonly HistoryViewModel _viewModel;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
    
    // Parameterless constructor for Shell DataTemplate
    public HistoryPage()
    {
        var viewModel = Application.Current?.Handler?.MauiContext?.Services?.GetService<HistoryViewModel>();
        if (viewModel == null) throw new InvalidOperationException("Could not resolve HistoryViewModel");
        _viewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
#pragma warning restore CA1416
}
