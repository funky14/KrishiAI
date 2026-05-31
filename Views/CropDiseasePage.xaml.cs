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
    
    // Parameterless constructor for Shell DataTemplate
    public CropDiseasePage() : this(IPlatformApplication.Current!.Services.GetService<CropDiseaseViewModel>()!)
    {
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}
