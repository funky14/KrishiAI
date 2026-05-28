using KrishiAI.App.Services;

namespace KrishiAI.App;

public partial class App : Application
{
    private readonly IConfigurationService _configService;

    public App(IConfigurationService configurationService)
    {
        InitializeComponent();
        _configService = configurationService;

        MainPage = new AppShell();
        
        // Initialize configuration immediately in constructor
        Task.Run(async () =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 APP CONSTRUCTOR: Starting azure_config.json initialization...");
                System.Diagnostics.Debug.WriteLine($"📂 App Data Directory: {FileSystem.AppDataDirectory}");
                
                var config = await _configService.GetConfigurationAsync();
                
                System.Diagnostics.Debug.WriteLine($"✅ Configuration initialized successfully!");
                System.Diagnostics.Debug.WriteLine($"📁 Config file location: {Path.Combine(FileSystem.AppDataDirectory, "azure_config.json")}");
                System.Diagnostics.Debug.WriteLine($"   - Speech configured: {!string.IsNullOrEmpty(config.SpeechServiceKey)}");
                System.Diagnostics.Debug.WriteLine($"   - OpenAI configured: {!string.IsNullOrEmpty(config.OpenAIKey)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ FATAL ERROR in config initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        });
    }

    protected override async void OnStart()
    {
        base.OnStart();
        
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 ONSTART: Verifying azure_config.json...");
            var config = await _configService.GetConfigurationAsync();
            System.Diagnostics.Debug.WriteLine($"✅ ONSTART: Config verified");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ONSTART ERROR: {ex.Message}");
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
