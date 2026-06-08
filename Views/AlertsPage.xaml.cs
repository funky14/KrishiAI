namespace KrishiAI.App.Views;

public partial class AlertsPage : ContentPage
{
    public AlertsPage(ViewModels.WeatherRiskViewModel viewModel)
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
