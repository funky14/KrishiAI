using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class FinanceVoiceEntryPage : ContentPage
{
    public FinanceVoiceEntryPage(FinanceVoiceEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            if (BindingContext is FinanceVoiceEntryViewModel viewModel)
            {
                // Ensure UI is fully loaded and we run on Main Thread
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), async () => 
                {
                    try 
                    {
                        await viewModel.SpeakGreetingAsync();
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in SpeakGreetingAsync: {innerEx.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        try
        {
            if (BindingContext is FinanceVoiceEntryViewModel viewModel)
            {
                viewModel.StopSpeech();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnDisappearing: {ex.Message}");
        }
    }
}
