using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using KrishiAI.App.Resources.Strings;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace KrishiAI.App.ViewModels;

public partial class HistoryViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private readonly IConnectivityService _connectivityService;
    private readonly SyncQueueManager _syncQueueManager;

    [ObservableProperty]
    private ObservableCollection<DiseaseDetectionResult> historyItems = new();

    [ObservableProperty]
    private bool hasHistory;

    [ObservableProperty]
    private bool isSyncing;

    [ObservableProperty]
    private string detectionHistoryText = string.Empty;

    [ObservableProperty]
    private string syncingDetectionHistoryText = string.Empty;

    [ObservableProperty]
    private string yourDetectionRecordsText = string.Empty;

    [ObservableProperty]
    private string trackAllYourCropDiseaseDetectionsText = string.Empty;

    [ObservableProperty]
    private string noHistoryYetText = string.Empty;

    [ObservableProperty]
    private string startAnalyzingCropsToSeeHistoryHereText = string.Empty;

    [ObservableProperty]
    private string viewDetailsText = string.Empty;

    [ObservableProperty]
    private string deleteItemText = string.Empty;

    [ObservableProperty]
    private string confidenceText = string.Empty;

    public HistoryViewModel(
        IDatabaseService databaseService,
        IConnectivityService connectivityService,
        SyncQueueManager syncQueueManager,
        ILocalizationService localizationService)
    {
        _databaseService = databaseService;
        _connectivityService = connectivityService;
        _syncQueueManager = syncQueueManager;

        InitializeLocalization(localizationService);

        Title = "Detection History";
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        Title = AppStrings.DetectionHistory;
        DetectionHistoryText = AppStrings.DetectionHistory;
        SyncingDetectionHistoryText = AppStrings.SyncingDetectionHistory;
        YourDetectionRecordsText = AppStrings.YourDetectionRecords;
        TrackAllYourCropDiseaseDetectionsText = AppStrings.TrackAllYourCropDiseaseDetections;
        NoHistoryYetText = AppStrings.NoHistoryYet;
        StartAnalyzingCropsToSeeHistoryHereText = AppStrings.StartAnalyzingCropsToSeeHistoryHere;
        ViewDetailsText = AppStrings.ViewDetails;
        DeleteItemText = AppStrings.DeleteItem;
        ConfidenceText = AppStrings.Confidence;
    }

    public override void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
    }

    public override async void OnAppearing()
    {
        base.OnAppearing();

        UpdateLocalizedStrings(); // Refresh translations when page appears

        // Subscribe to connectivity changes (Phase 4)
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;

        // Load from local store first (local-first)
        await LoadHistory();

        // Attempt sync after local load so UI is responsive, then refresh with latest state.
        await TriggerSyncAsync();
    }

    public override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from connectivity events to avoid memory leak
        _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
    }

    private async void OnConnectivityChanged(object? sender, bool isConnected)
    {
        Debug.WriteLine($"📡 Connectivity changed: {isConnected}");
        if (isConnected)
        {
            // Network restored - attempt sync
            await TriggerSyncAsync();
        }
    }

    private async Task TriggerSyncAsync()
    {
        try
        {
            IsSyncing = true;
            await _syncQueueManager.ProcessQueueAsync();
            
            // Refresh history after sync completes
            await LoadHistory();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Sync error: {ex.Message}");
        }
        finally
        {
            IsSyncing = false;
        }
    }

    [RelayCommand]
    private async Task LoadHistory()
    {
        try
        {
            IsBusy = true;
            var items = await _databaseService.GetHistoryAsync();
            HistoryItems.Clear();
            foreach (var item in items)
            {
                HistoryItems.Add(item);
            }
            HasHistory = HistoryItems.Any();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading history: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await TriggerSyncAsync();
    }

    [RelayCommand]
    private async Task DeleteItem(DiseaseDetectionResult item)
    {
        try
        {
            // Soft delete for sync (Phase 2)
            await _databaseService.SoftDeleteAsync(item);
            HistoryItems.Remove(item);
            HasHistory = HistoryItems.Any();
            
            // Attempt to sync deletion if online (Phase 4)
            if (_connectivityService.IsConnected())
            {
                await TriggerSyncAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting item: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ClearAllHistory()
    {
        try
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Clear History",
                "Are you sure you want to delete all history?",
                "Yes",
                "No");

            if (confirm)
            {
                await _databaseService.ClearHistoryAsync();
                HistoryItems.Clear();
                HasHistory = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error clearing history: {ex.Message}";
        }
    }
}
