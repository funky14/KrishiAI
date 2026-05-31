using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
    
    // Parameterless constructor for Shell DataTemplate
    public HistoryPage() : this(IPlatformApplication.Current!.Services.GetService<HistoryViewModel>()!)
    {
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}
