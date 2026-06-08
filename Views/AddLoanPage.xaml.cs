using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class AddLoanPage : ContentPage
{
    public AddLoanPage(AddLoanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
