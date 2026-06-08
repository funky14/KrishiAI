namespace KrishiAI.App.Views;

public partial class CropManagementPage : ContentPage
{
    public CropManagementPage(ViewModels.CropManagementViewModel viewModel)
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
