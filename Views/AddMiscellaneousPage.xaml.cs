using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class AddMiscellaneousPage : ContentPage
{
    public AddMiscellaneousPage(AddMiscellaneousViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
