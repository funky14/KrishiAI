using KrishiAI.App.Services;
using KrishiAI.App.Resources.Strings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using System;

namespace KrishiAI.App;

public partial class AppShell : Shell
{
    private readonly ILocalizationService _localizationService;

    public AppShell(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        InitializeShell();
    }

    // Parameterless constructor required for XAML tooling and when AppShell is created by XAML loader
    public AppShell()
    {
        // Try to resolve localization service from MAUI services; may be null during some startup phases
        try
        {
            _localizationService = Application.Current?.Handler?.MauiContext?.Services.GetService<ILocalizationService>() ?? new LocalizationService();
        }
        catch
        {
            _localizationService = new LocalizationService();
        }

        InitializeShell();
    }

    private void UpdateMenuTitles()
    {
        try
        {
            // Update by position to avoid issues where Route may not match during runtime
            foreach (var item in this.Items)
            {
                if (item is TabBar tabBar)
                {
                    try
                    {
                        // Ensure UI updates run on main thread (language change may be raised from background)
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (tabBar.Items.Count > 0) tabBar.Items[0].Title = AppStrings.Home;
                            if (tabBar.Items.Count > 1) tabBar.Items[1].Title = AppStrings.WeatherAndIrrigation;
                            if (tabBar.Items.Count > 2) tabBar.Items[2].Title = AppStrings.Disease;
                            if (tabBar.Items.Count > 3) tabBar.Items[3].Title = AppStrings.VoiceAssistant;
                            if (tabBar.Items.Count > 4) tabBar.Items[4].Title = AppStrings.History;
                            // Do not assume a sixth bottom tab (Android BottomNavigationView supports max 5).
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error assigning tab titles by index: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating menu titles: {ex.Message}");
        }
    }

    private void InitializeShell()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏗️ Initializing AppShell...");
            // Call XAML-generated InitializeComponent if available; guard for design-time where it may be missing
            try
            {
                InitializeComponent();
            }
            catch
            {
                // Design-time or situations where partial class XAML isn't generated yet
            }

            // Register routes for modal/flyout navigation
            Routing.RegisterRoute("irrigation", typeof(Views.IrrigationAdvisorPage));
            Routing.RegisterRoute("risks", typeof(Views.AlertsPage));
            Routing.RegisterRoute("crops", typeof(Views.CropManagementPage));
            Routing.RegisterRoute("weather/details", typeof(Views.WeatherDetailsPage));
            Routing.RegisterRoute("irrigation/schedule", typeof(Views.IrrigationAdvisorPage));
            // Register settings route (settings moved to toolbar to avoid >5 bottom tabs on Android)
            Routing.RegisterRoute("settings", typeof(Views.SettingsPage));

            // Subscribe to language changes to update tab titles
            if (_localizationService != null)
                _localizationService.LanguageChanged += (s, e) => UpdateMenuTitles();

            // Set initial titles
            UpdateMenuTitles();

            // Expose initial refresh for callers that resolve AppShell manually
            // (e.g., App constructor) to ensure titles are updated after MainPage assignment
            try { this.RefreshMenuTitles(); } catch { }

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
            System.Diagnostics.Debug.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");

            // Show detailed error
            if (ex.InnerException != null)
            {
                var inner = ex.InnerException;
                System.Diagnostics.Debug.WriteLine($"❌ INNER DETAILS: {inner.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"❌ INNER MESSAGE: {inner.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ INNER STACK: {inner.StackTrace}");
            }
            // Swallow exceptions to keep shell initialization resilient during startup.
            System.Diagnostics.Debug.WriteLine("❌ Initialization error swallowed to allow app to continue.");
        }
    }

    /// <summary>
    /// Public helper to refresh menu titles (kept separate for callers)
    /// </summary>
    public void RefreshMenuTitles()
    {
        UpdateMenuTitles();
    }

    private async void OnMoreClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("settings");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnMoreClicked navigation error: {ex.Message}");
        }
    }
}
