using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class FinanceVoiceEntryPage : ContentPage
{
    public FinanceVoiceEntryPage(FinanceVoiceEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
