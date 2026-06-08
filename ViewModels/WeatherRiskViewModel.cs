using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models.Risk;
using KrishiAI.App.Resources.Strings;
using KrishiAI.App.Services;
using KrishiAI.App.Services.Risk;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class WeatherRiskViewModel : BaseViewModel
{
    private readonly IRiskAnalysisService _riskAnalysis;

    [ObservableProperty]
    private ObservableCollection<WeatherRisk> activeRisks = new();

    [ObservableProperty]
    private bool hasRisks;

    [ObservableProperty]
    private bool hasCriticalRisks;

    [ObservableProperty]
    private int totalRisks;

    [ObservableProperty]
    private int criticalCount;

    [ObservableProperty]
    private int highCount;

    [ObservableProperty]
    private int moderateCount;

    public WeatherRiskViewModel(
        IRiskAnalysisService riskAnalysis,
        ILocalizationService localizationService)
    {
        _riskAnalysis = riskAnalysis;
        InitializeLocalization(localizationService);
        Title = AppStrings.WeatherRisks;
    }

    public override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRisksAsync();
    }

    [RelayCommand]
    private async Task LoadRisksAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var risks = await _riskAnalysis.GetActiveRisksAsync();

            ActiveRisks.Clear();
            foreach (var risk in risks)
            {
                ActiveRisks.Add(risk);
            }

            TotalRisks = ActiveRisks.Count;
            HasRisks = TotalRisks > 0;

            CriticalCount = ActiveRisks.Count(r => r.RiskLevel == RiskLevel.Critical);
            HighCount = ActiveRisks.Count(r => r.RiskLevel == RiskLevel.High);
            ModerateCount = ActiveRisks.Count(r => r.RiskLevel == RiskLevel.Moderate);

            HasCriticalRisks = CriticalCount > 0 || HighCount > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading risks: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AcknowledgeRiskAsync(WeatherRisk risk)
    {
        try
        {
            await _riskAnalysis.AcknowledgeRiskAsync(risk.Id);
            risk.IsAcknowledged = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
    }
}
