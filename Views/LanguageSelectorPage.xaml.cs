using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class LanguageSelectorPage : ContentPage
{
    public LanguageSelectorPage(LanguageSelectorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
