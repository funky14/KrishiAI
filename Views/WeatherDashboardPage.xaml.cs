namespace KrishiAI.App.Views;

public partial class WeatherDashboardPage : ContentPage
{
    public WeatherDashboardPage(ViewModels.WeatherDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.BaseViewModel viewModel)
        {
            viewModel.OnAppearing();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ViewModels.BaseViewModel viewModel)
        {
            viewModel.OnDisappearing();
        }
    }
}
