using KrishiAI.App.Views;
using KrishiAI.App.ViewModels;

namespace KrishiAI.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏗️ Initializing AppShell...");

            // Set BindingContext before InitializeComponent
            BindingContext = new AppShellViewModel();

            InitializeComponent();

            // Register routes for new pages
            Routing.RegisterRoute("notifications", typeof(NotificationsPage));
            Routing.RegisterRoute("farmertips", typeof(FarmerTipsPage));
            Routing.RegisterRoute("languageselector", typeof(LanguageSelectorPage));
            Routing.RegisterRoute("detectionresult", typeof(DetectionResultPage));

            // Add navigation event handlers
            this.Navigating += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"🧭 Navigating to: {e.Target.Location}");
            };

            this.Navigated += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"✅ Navigated to: {e.Current?.Location}");
            };

            System.Diagnostics.Debug.WriteLine("✅ AppShell initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ AppShell initialization error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
