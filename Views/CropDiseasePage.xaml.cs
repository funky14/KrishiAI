using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class CropDiseasePage : ContentPage
{
    private readonly CropDiseaseViewModel _viewModel;

    public CropDiseasePage(CropDiseaseViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}
