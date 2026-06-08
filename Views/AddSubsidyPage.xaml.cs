using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class AddSubsidyPage : ContentPage
{
    public AddSubsidyPage(AddSubsidyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
