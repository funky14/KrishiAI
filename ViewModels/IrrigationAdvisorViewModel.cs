using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models.Irrigation;
using KrishiAI.App.Models.Weather;
using KrishiAI.App.Services;
using KrishiAI.App.Services.Irrigation;
using KrishiAI.App.Services.Weather;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class IrrigationAdvisorViewModel : BaseViewModel
{
    private readonly IIrrigationService _irrigationService;
    private readonly IWeatherService _weatherService;
    private readonly IDatabaseService _database;

    [ObservableProperty]
    private CropInfo? activeCrop;

    [ObservableProperty]
    private IrrigationRecommendation? currentRecommendation;

    [ObservableProperty]
    private ObservableCollection<IrrigationRecommendation> history = new();

    [ObservableProperty]
    private double soilMoistureInput = 70.0;

    [ObservableProperty]
    private string soilMoistureStatus = "Optimal";

    [ObservableProperty]
    private string soilMoistureColor = "#4CAF50";

    [ObservableProperty]
    private bool hasRecommendation;

    [ObservableProperty]
    private bool hasActiveCrop;

    [ObservableProperty]
    private string irrigationStatusIcon = "💧";

    [ObservableProperty]
    private string irrigationStatusText = "No recommendation yet";

    public IrrigationAdvisorViewModel(
        IIrrigationService irrigationService,
        IWeatherService weatherService,
        IDatabaseService database,
        ILocalizationService localizationService)
    {
        _irrigationService = irrigationService;
        _weatherService = weatherService;
        _database = database;

        InitializeLocalization(localizationService);
        Title = "Irrigation Advisor";
    }

    public override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Load active crop
            ActiveCrop = await _database.GetActiveCropProfileAsync();
            HasActiveCrop = ActiveCrop != null;

            if (ActiveCrop != null)
            {
                SoilMoistureInput = ActiveCrop.SoilMoisturePercentage;
                UpdateSoilMoistureStatus();

                // Load recommendation
                CurrentRecommendation = await _irrigationService.GetLatestRecommendationAsync();
                HasRecommendation = CurrentRecommendation != null;

                if (CurrentRecommendation != null)
                {
                    UpdateIrrigationStatus();
                }

                // Load history
                var historyList = await _irrigationService.GetHistoryAsync(ActiveCrop.Id, 20);
                History.Clear();
                foreach (var item in historyList)
                {
                    History.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GenerateRecommendationAsync()
    {
        if (IsBusy || ActiveCrop == null) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Update crop's soil moisture
            ActiveCrop.SoilMoisturePercentage = SoilMoistureInput;
            await _database.SaveCropProfileAsync(ActiveCrop);

            // Get weather
            var weather = await _weatherService.GetCurrentLocationWeatherAsync();
            if (weather == null)
            {
                ErrorMessage = "Unable to fetch weather data";
                return;
            }

            // Generate recommendation
            CurrentRecommendation = await _irrigationService.GenerateRecommendationAsync(
                ActiveCrop,
                weather,
                SoilMoistureInput);

            HasRecommendation = CurrentRecommendation != null;

            if (CurrentRecommendation != null)
            {
                UpdateIrrigationStatus();

                // Reload history
                var historyList = await _irrigationService.GetHistoryAsync(ActiveCrop.Id, 20);
                History.Clear();
                foreach (var item in historyList)
                {
                    History.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error generating recommendation: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task MarkAsCompletedAsync()
    {
        if (CurrentRecommendation == null) return;

        try
        {
            await _irrigationService.MarkAsActionedAsync(CurrentRecommendation.Id, "Completed");
            CurrentRecommendation.UserActioned = true;
            await Shell.Current.DisplayAlert("Success", "Irrigation marked as completed!", "OK");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NavigateToCropSetupAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("crops");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Navigation error: {ex.Message}";
        }
    }

    partial void OnSoilMoistureInputChanged(double value)
    {
        UpdateSoilMoistureStatus();
    }

    private void UpdateSoilMoistureStatus()
    {
        if (ActiveCrop == null) return;

        if (SoilMoistureInput < ActiveCrop.MinSoilMoisture)
        {
            SoilMoistureStatus = "⚠️ Critical - Too Dry";
            SoilMoistureColor = "#F44336";
        }
        else if (SoilMoistureInput < ActiveCrop.OptimalSoilMoistureMin)
        {
            SoilMoistureStatus = "⚠️ Low - Needs Water";
            SoilMoistureColor = "#FF9800";
        }
        else if (SoilMoistureInput <= ActiveCrop.OptimalSoilMoistureMax)
        {
            SoilMoistureStatus = "✅ Optimal";
            SoilMoistureColor = "#4CAF50";
        }
        else
        {
            SoilMoistureStatus = "💧 High - Well Watered";
            SoilMoistureColor = "#2196F3";
        }
    }

    private void UpdateIrrigationStatus()
    {
        if (CurrentRecommendation == null) return;

        if (CurrentRecommendation.ShouldIrrigate)
        {
            IrrigationStatusIcon = CurrentRecommendation.Priority switch
            {
                "Critical" => "🚨",
                "High" => "⚠️",
                _ => "💧"
            };
            IrrigationStatusText = "Irrigation Recommended";
        }
        else
        {
            IrrigationStatusIcon = "✅";
            IrrigationStatusText = "No Irrigation Needed";
        }
    }
}
