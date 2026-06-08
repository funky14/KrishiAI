namespace KrishiAI.App.Views;

public partial class WeatherRiskPage : ContentPage
{
    public WeatherRiskPage(ViewModels.WeatherRiskViewModel viewModel)
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
}
