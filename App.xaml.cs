using KrishiAI.App.Services;
using KrishiAI.App.Views;
using KrishiAI.App.ViewModels;

namespace KrishiAI.App;

public partial class App : Application
{
    private readonly IConfigurationService _configService;
    private readonly ILocalizationService _localizationService;
    private readonly IAuthenticationService _authService;

    public App(IConfigurationService configurationService, ILocalizationService localizationService, IAuthenticationService authService)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🚀 APP STARTING...");

            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("   InitializeComponent() done");

            _configService = configurationService;
            _localizationService = localizationService;
            _authService = authService;
            System.Diagnostics.Debug.WriteLine("   Services assigned");

            // Initialize saved language preference
            var savedLanguage = Preferences.Get("AppLanguage", "en-US");
            _localizationService.SetCulture(savedLanguage);
            System.Diagnostics.Debug.WriteLine($"   Language set to: {savedLanguage}");

            // Set initial page based on authentication status
            SetInitialPage();
            System.Diagnostics.Debug.WriteLine("✅ APP CONSTRUCTOR COMPLETED - Initial page set");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ APP CONSTRUCTOR FAILED!");
            System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"❌ Inner: {ex.InnerException?.Message}");
            throw;
        }
    }

    private void SetInitialPage()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔍 SetInitialPage: Checking authentication status...");

            // Try to read from preferences first (synchronous) - faster and avoids deadlock
            var userIdStr = Preferences.Get("CurrentUserId", string.Empty);
            System.Diagnostics.Debug.WriteLine($"   - Preferences check: UserIdStr = '{userIdStr}'");

            if (!string.IsNullOrEmpty(userIdStr))
            {
                // User is authenticated - show app shell
                System.Diagnostics.Debug.WriteLine("   - User is authenticated, creating AppShell...");
                MainPage = new AppShell();
                System.Diagnostics.Debug.WriteLine("🔓 User authenticated - AppShell loaded");
            }
            else
            {
                // User not authenticated - show login page
                System.Diagnostics.Debug.WriteLine("   - User not authenticated, preparing LoginPage...");

                var authViewModel = IPlatformApplication.Current?.Services.GetService<AuthViewModel>();
                if (authViewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("   ❌ CRITICAL: AuthViewModel is null! DI not working.");
                    throw new InvalidOperationException("AuthViewModel not registered in DI container");
                }

                System.Diagnostics.Debug.WriteLine("   - AuthViewModel obtained, creating LoginPage...");
                var loginPage = new LoginPage(authViewModel);
                System.Diagnostics.Debug.WriteLine("   - LoginPage created, wrapping in NavigationPage...");

                var primaryColor = (Color)Application.Current!.Resources["Primary"];
                MainPage = new NavigationPage(loginPage)
                {
                    BarBackgroundColor = primaryColor
                };
                System.Diagnostics.Debug.WriteLine("🔒 User not authenticated - LoginPage loaded successfully");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ SetInitialPage FAILED!");
            System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"❌ Inner: {ex.InnerException?.Message}");

            // Show a visible error page instead of red blank
            MainPage = new ContentPage 
            { 
                BackgroundColor = Colors.White,
                Content = new VerticalStackLayout
                {
                    Padding = 20,
                    Spacing = 10,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Children = 
                    {
                        new Label { Text = "❌ App Initialization Error", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Colors.Red, HorizontalTextAlignment = TextAlignment.Center },
                        new Label { Text = ex.Message, FontSize = 14, TextColor = Colors.Black, HorizontalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.WordWrap },
                        new Label { Text = $"Type: {ex.GetType().Name}", FontSize = 12, TextColor = Colors.Gray },
                        new Label { Text = "Please restart the app", FontSize = 12, TextColor = Colors.Gray, HorizontalTextAlignment = TextAlignment.Center }
                    }
                }
            };
        }
    }

    protected override async void OnStart()
    {
        base.OnStart();

        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 ONSTART: Initializing azure_config.json...");
            System.Diagnostics.Debug.WriteLine($"📂 App Data Directory: {FileSystem.AppDataDirectory}");

            var config = await _configService.GetConfigurationAsync();

            System.Diagnostics.Debug.WriteLine($"✅ Configuration loaded!");
            System.Diagnostics.Debug.WriteLine($"   - Speech configured: {!string.IsNullOrEmpty(config.SpeechServiceKey)}");
            System.Diagnostics.Debug.WriteLine($"   - OpenAI configured: {!string.IsNullOrEmpty(config.OpenAIKey)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ONSTART ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Navigate to the main app shell after successful login
    /// </summary>
    public async Task NavigateToAppShellAsync()
    {
        MainPage = new AppShell();
        System.Diagnostics.Debug.WriteLine("✅ Navigated to AppShell after successful login");
    }

    /// <summary>
    /// Navigate back to login (for logout)
    /// </summary>
    public async Task NavigateToLoginAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 NavigateToLoginAsync: Starting logout navigation...");

            // Get fresh AuthViewModel
            var authViewModel = IPlatformApplication.Current?.Services.GetService<AuthViewModel>();
            if (authViewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ AuthViewModel not available");
                return;
            }

            // Create login page wrapped in NavigationPage
            var loginPage = new LoginPage(authViewModel);
            var navPage = new NavigationPage(loginPage)
            {
                BarBackgroundColor = (Color)Application.Current!.Resources["Primary"],
                BarTextColor = Colors.White
            };

            // Clear MainPage (removes AppShell completely) and set LoginPage
            MainPage = navPage;

            System.Diagnostics.Debug.WriteLine("✅ NavigateToLoginAsync: Successfully navigated to LoginPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ NavigateToLoginAsync Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);

        // Handle global exceptions - IMPROVED
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"❌❌❌ Unhandled Exception: {exception?.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Exception Type: {exception?.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack Trace: {exception?.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"❌ Inner Exception: {exception?.InnerException?.Message}");
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ Unobserved Task Exception: {e.Exception?.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Exception Type: {e.Exception?.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"❌ Inner Exception: {e.Exception?.InnerException?.Message}");
            e.SetObserved();
        };

        // Catch navigation errors
        Microsoft.Maui.Controls.Routing.RegisterRoute("voiceassistant", typeof(Views.VoiceAssistantPage));

        return window;
    }
}

