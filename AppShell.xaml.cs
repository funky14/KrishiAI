using KrishiAI.App.Views;

namespace KrishiAI.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏗️ Initializing AppShell...");
            InitializeComponent();
            
            // Add navigation event handlers
            this.Navigating += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"🧭 Navigating to: {e.Target.Location}");
            };
            
            this.Navigated += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"✅ Navigated to: {e.Current?.Location}");
            };

            Routing.RegisterRoute(nameof(AddExpensePage), typeof(AddExpensePage));
            Routing.RegisterRoute(nameof(AddIncomePage), typeof(AddIncomePage));
            Routing.RegisterRoute(nameof(AddLoanPage), typeof(AddLoanPage));
            Routing.RegisterRoute(nameof(AddSubsidyPage), typeof(AddSubsidyPage));
            Routing.RegisterRoute(nameof(AddMiscellaneousPage), typeof(AddMiscellaneousPage));
            Routing.RegisterRoute(nameof(FinanceVoiceEntryPage), typeof(FinanceVoiceEntryPage));
            Routing.RegisterRoute(nameof(FinanceHistoryPage), typeof(FinanceHistoryPage));
            Routing.RegisterRoute(nameof(FinanceReportsPage), typeof(FinanceReportsPage));
            Routing.RegisterRoute(nameof(ProfitSummaryPage), typeof(ProfitSummaryPage));
            Routing.RegisterRoute(nameof(AiInsightsPage), typeof(AiInsightsPage));
            
            System.Diagnostics.Debug.WriteLine("✅ AppShell initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ AppShell initialization error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");
            
            // Show detailed error
            if (ex.InnerException != null)
            {
                var inner = ex.InnerException;
                System.Diagnostics.Debug.WriteLine($"❌ INNER DETAILS: {inner.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"❌ INNER MESSAGE: {inner.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ INNER STACK: {inner.StackTrace}");
            }
            throw;
        }
    }
}
