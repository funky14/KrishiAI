using KrishiAI.App.Services;
using KrishiAI.App.ViewModels;
using KrishiAI.App.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microcharts.Maui;

namespace KrishiAI.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMicrocharts();
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
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
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
        builder.Services.AddSingleton<IFinanceAzureSqlService, FinanceAzureSqlService>(); // Azure SQL backend for Finance
        builder.Services.AddSingleton<IFinanceService, FinanceService>();                  // Connectivity-aware router (SQLite offline / Azure SQL online)
        builder.Services.AddSingleton<FinanceSyncService>();                               // Pushes offline SQLite records to Azure SQL on reconnect
        
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
        builder.Services.AddSingleton<FinanceViewModel>();

        // Finance entry ViewModels (Transient so forms are empty each time)
        builder.Services.AddTransient<AddExpenseViewModel>();
        builder.Services.AddTransient<AddIncomeViewModel>();
        builder.Services.AddTransient<AddLoanViewModel>();
        builder.Services.AddTransient<AddSubsidyViewModel>();
        builder.Services.AddTransient<AddMiscellaneousViewModel>();
        builder.Services.AddTransient<FinanceVoiceEntryViewModel>();
        builder.Services.AddTransient<FinanceHistoryViewModel>();
        builder.Services.AddTransient<FinanceReportsViewModel>();
        builder.Services.AddTransient<ProfitSummaryViewModel>();
        builder.Services.AddTransient<AiInsightsViewModel>();

        // Register Views with DI
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<CropDiseasePage>();
        builder.Services.AddSingleton<VoiceAssistantPage>();
        builder.Services.AddSingleton<HistoryPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<FinancePage>();

        // Finance entry Views (Transient)
        builder.Services.AddTransient<AddExpensePage>();
        builder.Services.AddTransient<AddIncomePage>();
        builder.Services.AddTransient<AddLoanPage>();
        builder.Services.AddTransient<AddSubsidyPage>();
        builder.Services.AddTransient<AddMiscellaneousPage>();
        builder.Services.AddTransient<FinanceVoiceEntryPage>();
        builder.Services.AddTransient<FinanceHistoryPage>();
        builder.Services.AddTransient<FinanceReportsPage>();
        builder.Services.AddTransient<ProfitSummaryPage>();
        builder.Services.AddTransient<AiInsightsPage>();

        return builder.Build();
    }
}
