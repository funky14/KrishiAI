using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class FinanceVoiceEntryPage : ContentPage
{
    public FinanceVoiceEntryPage(FinanceVoiceEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is FinanceVoiceEntryViewModel viewModel)
        {
            await viewModel.SpeakGreetingAsync();
        }
    }
}
