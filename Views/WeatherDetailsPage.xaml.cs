namespace KrishiAI.App.Views;

public partial class WeatherDetailsPage : ContentPage
{
    public WeatherDetailsPage(ViewModels.WeatherDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.BaseViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}
