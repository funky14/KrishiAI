using KrishiAI.App.ViewModels;

namespace KrishiAI.App.Views;

public partial class HomePage : ContentPage
{
#pragma warning disable CA1416
    public HomePage(HomeViewModel viewModel)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏠 Creating HomePage...");
            InitializeComponent();
            BindingContext = viewModel;
            System.Diagnostics.Debug.WriteLine("✅ HomePage created successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ HomePage creation failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
            throw;
        }
    }
    
    // Parameterless constructor for Shell DataTemplate
    public HomePage()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏠 Creating HomePage (parameterless)...");
            var handler = Application.Current?.Handler?.MauiContext?.Services;
            if (handler == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ Handler is NULL!");
                throw new InvalidOperationException("MauiContext.Services is not available");
            }
            
            var viewModel = handler.GetService<HomeViewModel>();
            if (viewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ HomeViewModel is NULL!");
                throw new InvalidOperationException("Could not resolve HomeViewModel from DI");
            }
            
            System.Diagnostics.Debug.WriteLine("📦 ViewModel resolved successfully");
            InitializeComponent();
            BindingContext = viewModel;
            System.Diagnostics.Debug.WriteLine("✅ HomePage created successfully (parameterless)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ HomePage parameterless constructor failed!");
            System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
            throw;
        }
    }
#pragma warning restore CA1416
}
