using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class DetectionResultPage : ContentPage
{
    public DetectionResultPage(DetectionResultViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
