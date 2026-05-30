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

#if DEBUG
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

        // Register Services
        builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<ICameraService, CameraService>();
        builder.Services.AddSingleton<ICropDiseaseAIService, CropDiseaseAIService>();
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();
        builder.Services.AddSingleton<ISpeechRecognitionService, SpeechRecognitionService>();
        builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
        builder.Services.AddSingleton<IAIChatService, AIChatService>();
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

        // Register ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<CropDiseaseViewModel>();
        builder.Services.AddTransient<VoiceAssistantViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<FarmerTipsViewModel>();
        builder.Services.AddTransient<LanguageSelectorViewModel>();
        builder.Services.AddTransient<DetectionResultViewModel>();

        // Register Views
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<CropDiseasePage>();
        builder.Services.AddTransient<VoiceAssistantPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<FarmerTipsPage>();
        builder.Services.AddTransient<LanguageSelectorPage>();
        builder.Services.AddTransient<DetectionResultPage>();

        return builder.Build();
    }
}
