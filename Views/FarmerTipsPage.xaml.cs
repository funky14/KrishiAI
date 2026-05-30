using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class FarmerTipsPage : ContentPage
{
    public FarmerTipsPage(FarmerTipsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
