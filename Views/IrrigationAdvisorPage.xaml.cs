namespace KrishiAI.App.Views;

public partial class IrrigationAdvisorPage : ContentPage
{
    public IrrigationAdvisorPage(ViewModels.IrrigationAdvisorViewModel viewModel)
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
