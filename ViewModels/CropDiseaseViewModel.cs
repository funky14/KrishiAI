using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class CropDiseaseViewModel : BaseViewModel
{
    private readonly ICameraService _cameraService;
    private readonly ICropDiseaseAIService _aiService;
    private readonly IRecommendationService _recommendationService;
    private readonly IDatabaseService _databaseService;
    private readonly DeviceIdentifierService _deviceIdentifierService;
    private readonly SyncQueueManager _syncQueueManager;

    [ObservableProperty]
    private string? selectedImagePath;

    [ObservableProperty]
    private DiseaseDetectionResult? detectionResult;

    [ObservableProperty]
    private DiseaseRecommendation? recommendation;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private bool hasImage;

    [ObservableProperty]
    private bool hasResult;

    public CropDiseaseViewModel(
        ICameraService cameraService,
        ICropDiseaseAIService aiService,
        IRecommendationService recommendationService,
        IDatabaseService databaseService,
        DeviceIdentifierService deviceIdentifierService,
        SyncQueueManager syncQueueManager)
    {
        _cameraService = cameraService;
        _aiService = aiService;
        _recommendationService = recommendationService;
        _databaseService = databaseService;
        _deviceIdentifierService = deviceIdentifierService;
        _syncQueueManager = syncQueueManager;
        
        Title = "Crop Disease Detection";
    }

    public override async void OnAppearing()
    {
        base.OnAppearing();
        await _aiService.InitializeAsync();
        await _recommendationService.InitializeAsync();
    }

    [RelayCommand]
    private async Task CapturePhoto()
    {
        try
        {
            var imagePath = await _cameraService.CapturePhotoAsync();
            if (!string.IsNullOrEmpty(imagePath))
            {
                SelectedImagePath = imagePath;
                HasImage = true;
                HasResult = false;
                DetectionResult = null;
                Recommendation = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error capturing photo: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PickPhoto()
    {
        try
        {
            var imagePath = await _cameraService.PickPhotoAsync();
            if (!string.IsNullOrEmpty(imagePath))
            {
                SelectedImagePath = imagePath;
                HasImage = true;
                HasResult = false;
                DetectionResult = null;
                Recommendation = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error picking photo: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AnalyzeImage()
    {
        if (string.IsNullOrEmpty(SelectedImagePath))
            return;

        try
        {
            IsAnalyzing = true;
            ErrorMessage = string.Empty;

            // Predict disease
            var result = await _aiService.PredictDiseaseAsync(SelectedImagePath);
            if (result != null)
            {
                DetectionResult = result;
                
                // Get recommendations
                var rec = await _recommendationService.GetRecommendationAsync(result.DiseaseName);
                if (rec != null)
                {
                    Recommendation = rec;
                    result.TreatmentRecommendations = rec.OrganicTreatment
                        .Concat(rec.ChemicalTreatment)
                        .Concat(rec.PreventionTips)
                        .ToList();
                }

                var deviceInfo = await _deviceIdentifierService.GetDeviceInfoAsync();
                result.DeviceId = deviceInfo.DeviceId;
                result.DeviceName = deviceInfo.DeviceName;
                result.LastModifiedDateUtc = DateTime.UtcNow;

                HasResult = true;

                // Save to database (Phase 2 - local-first)
                await _databaseService.SaveDetectionAsync(result);
                
                // Attempt immediate sync if online (Phase 5)
                _ = _syncQueueManager.ProcessQueueAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Analysis failed: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private void ClearResult()
    {
        SelectedImagePath = null;
        DetectionResult = null;
        Recommendation = null;
        HasImage = false;
        HasResult = false;
        ErrorMessage = string.Empty;
    }
}
