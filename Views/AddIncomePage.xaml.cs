using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class AddIncomePage : ContentPage
{
    public AddIncomePage(AddIncomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
