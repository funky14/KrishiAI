using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class VoiceAssistantPage : ContentPage
{
#pragma warning disable CA1416
    public VoiceAssistantPage(VoiceAssistantViewModel viewModel)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎤 Creating VoiceAssistantPage...");
            System.Diagnostics.Debug.WriteLine($"   ViewModel: {viewModel?.GetType().Name ?? "NULL"}");
            
            InitializeComponent();
            
            System.Diagnostics.Debug.WriteLine("   InitializeComponent() completed");
            
            BindingContext = viewModel;
            
            System.Diagnostics.Debug.WriteLine("✅ VoiceAssistantPage created successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ VoiceAssistantPage creation FAILED!");
            System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"❌ Inner: {ex.InnerException?.Message}");
            throw;
        }
    }
    
    // Parameterless constructor for Shell DataTemplate
    public VoiceAssistantPage()
    {
        var viewModel = Application.Current?.Handler?.MauiContext?.Services?.GetService<VoiceAssistantViewModel>();
        if (viewModel == null) throw new InvalidOperationException("Could not resolve VoiceAssistantViewModel");
        
        System.Diagnostics.Debug.WriteLine("🎤 Creating VoiceAssistantPage (parameterless)...");
        InitializeComponent();
        BindingContext = viewModel;
        System.Diagnostics.Debug.WriteLine("✅ VoiceAssistantPage created successfully (parameterless)");
    }
#pragma warning restore CA1416
}
