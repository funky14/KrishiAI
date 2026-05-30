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

    // Localized strings
    [ObservableProperty]
    private string noImageSelectedText = Resources.Strings.AppStrings.NoImageSelected;

    [ObservableProperty]
    private string captureOrSelectImageText = Resources.Strings.AppStrings.CaptureOrSelectImage;

    [ObservableProperty]
    private string captureText = Resources.Strings.AppStrings.Capture;

    [ObservableProperty]
    private string galleryText = Resources.Strings.AppStrings.Gallery;

    [ObservableProperty]
    private string analyzeDiseaseText = Resources.Strings.AppStrings.AnalyzeDisease;

    [ObservableProperty]
    private string analyzingImageText = Resources.Strings.AppStrings.AnalyzingImage;

    [ObservableProperty]
    private string detectionResultsText = Resources.Strings.AppStrings.DetectionResults;

    [ObservableProperty]
    private string confidenceText = Resources.Strings.AppStrings.Confidence;

    [ObservableProperty]
    private string severityText = Resources.Strings.AppStrings.Severity;

    [ObservableProperty]
    private string treatmentRecommendationsText = Resources.Strings.AppStrings.TreatmentRecommendations;

    [ObservableProperty]
    private string organicTreatmentText = Resources.Strings.AppStrings.OrganicTreatment;

    [ObservableProperty]
    private string chemicalTreatmentText = Resources.Strings.AppStrings.ChemicalTreatment;

    [ObservableProperty]
    private string preventionTipsText = Resources.Strings.AppStrings.PreventionTips;

    [ObservableProperty]
    private string newAnalysisText = Resources.Strings.AppStrings.NewAnalysis;

    // Computed properties for displaying localized labels with values
    public string ConfidenceDisplay => DetectionResult != null 
        ? $"{ConfidenceText}: {DetectionResult.Confidence:F1}%" 
        : "";

    public string SeverityDisplay => DetectionResult != null 
        ? $"{SeverityText}: {DetectionResult.Severity}" 
        : "";

    public CropDiseaseViewModel(
        ICameraService cameraService,
        ICropDiseaseAIService aiService,
        IRecommendationService recommendationService,
        IDatabaseService databaseService)
    {
        _cameraService = cameraService;
        _aiService = aiService;
        _recommendationService = recommendationService;
        _databaseService = databaseService;

        Title = Resources.Strings.AppStrings.CropDiseaseDetection;
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        Title = Resources.Strings.AppStrings.CropDiseaseDetection;
        UpdateLocalizedStrings();
        System.Diagnostics.Debug.WriteLine("🌍 CropDiseaseViewModel: Language changed");
    }

    private void UpdateLocalizedStrings()
    {
        NoImageSelectedText = Resources.Strings.AppStrings.NoImageSelected;
        CaptureOrSelectImageText = Resources.Strings.AppStrings.CaptureOrSelectImage;
        CaptureText = Resources.Strings.AppStrings.Capture;
        GalleryText = Resources.Strings.AppStrings.Gallery;
        AnalyzeDiseaseText = Resources.Strings.AppStrings.AnalyzeDisease;
        AnalyzingImageText = Resources.Strings.AppStrings.AnalyzingImage;
        DetectionResultsText = Resources.Strings.AppStrings.DetectionResults;
        ConfidenceText = Resources.Strings.AppStrings.Confidence;
        SeverityText = Resources.Strings.AppStrings.Severity;
        TreatmentRecommendationsText = Resources.Strings.AppStrings.TreatmentRecommendations;
        OrganicTreatmentText = Resources.Strings.AppStrings.OrganicTreatment;
        ChemicalTreatmentText = Resources.Strings.AppStrings.ChemicalTreatment;
        PreventionTipsText = Resources.Strings.AppStrings.PreventionTips;
        NewAnalysisText = Resources.Strings.AppStrings.NewAnalysis;

        // Refresh computed properties
        OnPropertyChanged(nameof(ConfidenceDisplay));
        OnPropertyChanged(nameof(SeverityDisplay));
    }

    partial void OnDetectionResultChanged(DiseaseDetectionResult? value)
    {
        OnPropertyChanged(nameof(ConfidenceDisplay));
        OnPropertyChanged(nameof(SeverityDisplay));
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
                    result.OrganicTreatments = rec.OrganicTreatment;
                    result.PreventionTips = rec.PreventionTips;
                }

                HasResult = true;

                // Save to database
                await _databaseService.SaveDetectionAsync(result);
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
