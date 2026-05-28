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
    }

    protected override async void OnStart()
    {
        base.OnStart();
        
        // Force initialization of azure_config.json on app startup
        System.Diagnostics.Debug.WriteLine("📁 Initializing azure_config.json...");
        var config = await _configService.GetConfigurationAsync();
        System.Diagnostics.Debug.WriteLine($"✅ Configuration initialized at: {Path.Combine(FileSystem.AppDataDirectory, "azure_config.json")}");
        System.Diagnostics.Debug.WriteLine($"   - Speech configured: {!string.IsNullOrEmpty(config.SpeechServiceKey)}");
        System.Diagnostics.Debug.WriteLine($"   - OpenAI configured: {!string.IsNullOrEmpty(config.OpenAIKey)}");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);

        // Handle global exceptions
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"Unhandled Exception: {exception?.Message}");
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"Unobserved Task Exception: {e.Exception?.Message}");
            e.SetObserved();
        };

        return window;
    }
}
