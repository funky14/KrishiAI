using KrishiAI.App.Services;
using KrishiAI.App.ViewModels;
using KrishiAI.App.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace KrishiAI.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();
            // Fonts commented out - add font files to Resources/Fonts/ to enable
            //.ConfigureFonts(fonts =>
            //{
            //    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            //    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            //});

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register Services
        builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>(); // Offline-first SQLite; SyncQueueManager handles cloud sync when online
        builder.Services.AddSingleton<ICameraService, CameraService>();
        builder.Services.AddSingleton<ICropDiseaseAIService, CropDiseaseAIService>();
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();
        builder.Services.AddSingleton<ISpeechRecognitionService, SpeechRecognitionService>();
        builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
        builder.Services.AddSingleton<IAIChatService, AIChatService>();
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        builder.Services.AddSingleton<DeviceIdentifierService>();
        
        // Register sync services (Phase 3)
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IHistorySyncService, HistorySyncService>();
        builder.Services.AddSingleton<SyncQueueManager>();

        // Register ViewModels
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<CropDiseaseViewModel>();
        builder.Services.AddSingleton<VoiceAssistantViewModel>();
        builder.Services.AddSingleton<HistoryViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

        // Register Views with DI
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<CropDiseasePage>();
        builder.Services.AddSingleton<VoiceAssistantPage>();
        builder.Services.AddSingleton<HistoryPage>();
        builder.Services.AddSingleton<SettingsPage>();

        return builder.Build();
    }
}
