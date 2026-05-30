using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;

namespace KrishiAI.App.ViewModels;

public partial class DetectionResultViewModel : BaseViewModel
{
    [ObservableProperty]
    private DiseaseDetectionResult? result;

    [ObservableProperty]
    private string diagnosisTitle = "Diagnosis";

    [ObservableProperty]
    private string organicTreatmentTitle = "🌿 Organic Treatment";

    [ObservableProperty]
    private string preventionTipsTitle = "🔵 Prevention Tips";

    public DetectionResultViewModel()
    {
        Title = Resources.Strings.AppStrings.DetectionResults;
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        Title = Resources.Strings.AppStrings.DetectionResults;
        System.Diagnostics.Debug.WriteLine("🌍 DetectionResultViewModel: Language changed");
    }

    public void Initialize(DiseaseDetectionResult detectionResult)
    {
        Result = detectionResult;
    }

    [RelayCommand]
    private async Task NewAnalysis()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}
