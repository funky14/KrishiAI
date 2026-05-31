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
    public CropDiseasePage()
    {
        var viewModel = Application.Current?.Handler?.MauiContext?.Services?.GetService<CropDiseaseViewModel>();
        if (viewModel == null) throw new InvalidOperationException("Could not resolve CropDiseaseViewModel");
        _viewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}
