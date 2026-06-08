using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class AddExpensePage : ContentPage
{
    public AddExpensePage(AddExpenseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
