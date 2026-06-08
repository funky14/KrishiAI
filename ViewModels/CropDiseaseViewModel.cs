using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using KrishiAI.App.Resources.Strings;
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

    [ObservableProperty]
    private string cropDiseaseDetectionText = string.Empty;

    [ObservableProperty]
    private string tapToCaptureOrSelectText = string.Empty;

    [ObservableProperty]
    private string aCropImageText = string.Empty;

    [ObservableProperty]
    private string capturePhotoText = string.Empty;

    [ObservableProperty]
    private string chooseFromGalleryText = string.Empty;

    [ObservableProperty]
    private string analyzeDiseaseText = string.Empty;

    [ObservableProperty]
    private string analyzingCropImageText = string.Empty;

    [ObservableProperty]
    private string diagnosisText = string.Empty;

    [ObservableProperty]
    private string diseaseText = string.Empty;

    [ObservableProperty]
    private string confidenceText = string.Empty;

    [ObservableProperty]
    private string recommendationsText = string.Empty;

    [ObservableProperty]
    private string organicTreatmentText = string.Empty;

    [ObservableProperty]
    private string chemicalTreatmentText = string.Empty;

    [ObservableProperty]
    private string preventionTipsText = string.Empty;

    public string ConfidenceDisplay => DetectionResult != null 
        ? $"{ConfidenceText}: {DetectionResult.Confidence:F1}%" 
        : string.Empty;

    public CropDiseaseViewModel(
        ICameraService cameraService,
        ICropDiseaseAIService aiService,
        IRecommendationService recommendationService,
        IDatabaseService databaseService,
        DeviceIdentifierService deviceIdentifierService,
        SyncQueueManager syncQueueManager,
        ILocalizationService localizationService)
    {
        _cameraService = cameraService;
        _aiService = aiService;
        _recommendationService = recommendationService;
        _databaseService = databaseService;
        _deviceIdentifierService = deviceIdentifierService;
        _syncQueueManager = syncQueueManager;

        InitializeLocalization(localizationService);

        Title = "Crop Disease Detection";
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        Title = AppStrings.CropDiseaseDetection;
        CropDiseaseDetectionText = AppStrings.CropDiseaseDetection;
        TapToCaptureOrSelectText = AppStrings.TapToCaptureOrSelect;
        ACropImageText = AppStrings.ACropImage;
        CapturePhotoText = AppStrings.CapturePhoto;
        ChooseFromGalleryText = AppStrings.ChooseFromGallery;
        AnalyzeDiseaseText = AppStrings.AnalyzeDisease;
        AnalyzingCropImageText = AppStrings.AnalyzingCropImage;
        DiagnosisText = AppStrings.Diagnosis;
        DiseaseText = AppStrings.Disease;
        ConfidenceText = AppStrings.Confidence;
        RecommendationsText = AppStrings.Recommendations;
        OrganicTreatmentText = AppStrings.OrganicTreatment;
        ChemicalTreatmentText = AppStrings.ChemicalTreatment;
        PreventionTipsText = AppStrings.PreventionTips;
        OnPropertyChanged(nameof(ConfidenceDisplay));
    }

    public override void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
    }

    public override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateLocalizedStrings(); // Refresh strings when page appears
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
                OnPropertyChanged(nameof(ConfidenceDisplay));

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
