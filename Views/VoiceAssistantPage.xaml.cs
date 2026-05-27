using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class VoiceAssistantPage : ContentPage
{
    public VoiceAssistantPage(VoiceAssistantViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
