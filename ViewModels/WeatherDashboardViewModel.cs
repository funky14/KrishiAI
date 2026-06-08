using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models.Weather;
using KrishiAI.App.Models.Irrigation;
using KrishiAI.App.Models.Risk;
using KrishiAI.App.Services;
using KrishiAI.App.Services.Weather;
using KrishiAI.App.Services.Irrigation;
using KrishiAI.App.Services.Risk;
using KrishiAI.App.Services.Location;
using System.Collections.ObjectModel;
using KrishiAI.App.Resources.Strings;
using System.Linq;

namespace KrishiAI.App.ViewModels;

public partial class WeatherDashboardViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private readonly IIrrigationService _irrigationService;
    private readonly IRiskAnalysisService _riskAnalysis;
    private readonly ILocationService _locationService;
    private readonly IDatabaseService _database;

    [ObservableProperty]
    private WeatherForecast? currentWeather;

    [ObservableProperty]
    private CurrentWeather? todayWeather;

    [ObservableProperty]
    private IrrigationRecommendation? latestRecommendation;

    [ObservableProperty]
    private CropInfo? activeCrop;

    [ObservableProperty]
    private ObservableCollection<WeatherRisk> activeRisks = new();

    [ObservableProperty]
    private ObservableCollection<DailyForecast> weeklyForecast = new();

    [ObservableProperty]
    private ObservableCollection<HourlyForecast> hourlyForecast = new();

    [ObservableProperty]
    private string locationName = "Loading...";

    [ObservableProperty]
    private string currentWeatherText = string.Empty;

    [ObservableProperty]
    private string riskAlertsText = string.Empty;

    [ObservableProperty]
    private string sevenDayForecastText = string.Empty;

    [ObservableProperty]
    private string manageCropsText = string.Empty;

    [ObservableProperty]
    private string refreshText = string.Empty;

    [ObservableProperty]
    private string viewAllRisksText = string.Empty;

    [ObservableProperty]
    private string temperatureText = string.Empty;

    [ObservableProperty]
    private string humidityText = string.Empty;

    [ObservableProperty]
    private string rainfallText = string.Empty;

    [ObservableProperty]
    private string windSpeedText = string.Empty;

    [ObservableProperty]
    private string maxTempText = string.Empty;

    [ObservableProperty]
    private string minTempText = string.Empty;

    [ObservableProperty]
    private string feelsLikeText = string.Empty;

    [ObservableProperty]
    private string humidityLabel = string.Empty;

    [ObservableProperty]
    private string windLabel = string.Empty;

    [ObservableProperty]
    private string feelsLikeLabel = string.Empty;

    [ObservableProperty]
    private string irrigationAdvisorText = string.Empty;

    [ObservableProperty]
    private string recommendedText = string.Empty;

    [ObservableProperty]
    private string irrigationDescriptionText = string.Empty;

    [ObservableProperty]
    private string cropLabel = string.Empty;

    [ObservableProperty]
    private string growthStageLabel = string.Empty;

    [ObservableProperty]
    private string soilMoistureLabel = string.Empty;

    [ObservableProperty]
    private string viewIrrigationPlanText = string.Empty;

    [ObservableProperty]
    private string weatherAndIrrigationText = string.Empty;

    [ObservableProperty]
    private string radiusText = string.Empty;

    [ObservableProperty]
    private string viewAllText = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool hasWeatherData;

    [ObservableProperty]
    private bool hasCropData;

    [ObservableProperty]
    private bool hasRisks;

    [ObservableProperty]
    private string weatherIcon = "☀️";

    [ObservableProperty]
    private string lastUpdatedText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>
    /// Computed property for the active crop's growth stage display name
    /// </summary>
    public string ActiveCropGrowthStageDisplay 
    {
        get => ActiveCrop?.GrowthStage.GetDisplayName() ?? string.Empty;
    }

    [RelayCommand]
    private async Task ViewAllAsync()
    {
        try
        {
            // Navigate to a detailed forecast page if registered, fallback to showing an alert
            await Shell.Current.GoToAsync("/weather/details");
        }
        catch
        {
            await Application.Current!.MainPage!.DisplayAlert(AppStrings.SevenDayForecast, AppStrings.ViewAll, AppStrings.OK);
        }
    }

    public WeatherDashboardViewModel(
        IWeatherService weatherService,
        IIrrigationService irrigationService,
        IRiskAnalysisService riskAnalysis,
        ILocationService locationService,
        IDatabaseService database,
        ILocalizationService localizationService)
    {
        _weatherService = weatherService;
        _irrigationService = irrigationService;
        _riskAnalysis = riskAnalysis;
        _locationService = locationService;
        _database = database;

        InitializeLocalization(localizationService);
        // Set initial localized title
        Title = AppStrings.WeatherAndIrrigation;
    }

    public override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateLocalizedStrings();
        await LoadDashboardAsync();
    }

    public override void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        // Use AppStrings GetString for dynamic keys added at runtime
        CurrentWeatherText = AppStrings.GetString("CurrentWeather");
        RiskAlertsText = AppStrings.GetString("WeatherRisks");
        SevenDayForecastText = AppStrings.GetString("SevenDayForecast");
        ManageCropsText = AppStrings.GetString("ManageCrops");
        RefreshText = AppStrings.GetString("Refresh");
        ViewAllRisksText = AppStrings.GetString("ViewAllRisks");

        TemperatureText = AppStrings.GetString("Temperature");
        HumidityText = AppStrings.GetString("Humidity");
        RainfallText = AppStrings.GetString("Rainfall");
        WindSpeedText = AppStrings.GetString("WindSpeed");

        // New strings for Weather & Irrigation UI
        MaxTempText = AppStrings.GetString("Max");
        MinTempText = AppStrings.GetString("Min");
        FeelsLikeText = AppStrings.GetString("FeelsLike");
        HumidityLabel = AppStrings.GetString("Humidity_Upper");
        WindLabel = AppStrings.GetString("Wind");
        FeelsLikeLabel = AppStrings.GetString("FeelsLikeValue");

        IrrigationAdvisorText = AppStrings.GetString("AIIrrigationAdvisor");
        RecommendedText = AppStrings.GetString("Recommended");
        IrrigationDescriptionText = AppStrings.GetString("IrrigateTomorrowMorning");

        CropLabel = AppStrings.GetString("Crop");
        GrowthStageLabel = AppStrings.GetString("GrowthStage");
        SoilMoistureLabel = AppStrings.GetString("SoilMoisture");

        ViewIrrigationPlanText = AppStrings.GetString("ViewIrrigationPlan");

        // Additional strings
        WeatherAndIrrigationText = AppStrings.GetString("WeatherAndIrrigation");
        RadiusText = AppStrings.GetString("Radius");
        ViewAllText = AppStrings.GetString("ViewAll");

        // Ensure ViewAll command localized label is set
        // The generated RelayCommand will be available as ViewAllCommand

        // Update Title as well
        Title = AppStrings.GetString("WeatherAndIrrigation");
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Load weather data
            await LoadWeatherDataAsync();

            // Load crop data
            await LoadCropDataAsync();

            // Load risks
            await LoadRisksAsync();

            // Generate irrigation recommendation if we have both weather and crop
            if (HasWeatherData && HasCropData && CurrentWeather != null && ActiveCrop != null)
            {
                LatestRecommendation = await _irrigationService.GenerateRecommendationAsync(
                    ActiveCrop,
                    CurrentWeather);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshWeatherAsync()
    {
        if (IsRefreshing) return;

        try
        {
            IsRefreshing = true;

            var location = await _locationService.GetCurrentLocationAsync();
            if (location != null)
            {
                // Do not pass the placeholder name from LocationService (e.g. "Current Location")
                // so WeatherService/OpenMeteoClient can perform reverse geocoding and return
                // a friendly City, State, Country formatted location name.
                CurrentWeather = await _weatherService.RefreshWeatherAsync(
                    location.Latitude,
                    location.Longitude,
                    location.LocationName);

                if (CurrentWeather != null)
                {
                    await UpdateDashboardDisplay();

                    // Analyze risks
                    var settings = new Models.AppSettings(); // Load from preferences in production
                    await _riskAnalysis.AnalyzeWeatherRisksAsync(CurrentWeather, settings.WeatherThresholds);
                    await LoadRisksAsync();

                    LastUpdatedText = $"Updated {DateTime.Now:h:mm tt}";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Refresh failed: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToIrrigationAdvisorAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("irrigation");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Navigation error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NavigateToRisksAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("risks");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Navigation error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NavigateToCropManagementAsync()
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

    private async Task LoadWeatherDataAsync()
    {
        CurrentWeather = await _weatherService.GetCurrentLocationWeatherAsync();

        if (CurrentWeather == null)
        {
            CurrentWeather = await _weatherService.GetLatestCachedWeatherAsync();
        }

        if (CurrentWeather != null)
        {
            await UpdateDashboardDisplay();
        }

        HasWeatherData = CurrentWeather != null;
    }

    private async Task UpdateDashboardDisplay()
    {
        if (CurrentWeather == null) return;

        LocationName = CurrentWeather.LocationName;
        TodayWeather = CurrentWeather.Current;

        if (TodayWeather != null)
        {
            WeatherIcon = TodayWeather.Icon;
        }

        // Update hourly forecast
        HourlyForecast.Clear();
        foreach (var hour in CurrentWeather.HourlyForecasts.Take(24))
        {
            HourlyForecast.Add(hour);
        }

        // Update weekly forecast
        WeeklyForecast.Clear();
        foreach (var day in CurrentWeather.DailyForecasts.Take(7))
        {
            WeeklyForecast.Add(day);
        }

        LastUpdatedText = CurrentWeather.IsFromCache 
            ? $"Cached: {CurrentWeather.FetchedAt:h:mm tt}" 
            : $"Live: {DateTime.Now:h:mm tt}";
    }

    private async Task LoadCropDataAsync()
    {
        ActiveCrop = await _database.GetActiveCropProfileAsync();
        HasCropData = ActiveCrop != null;
    }

    private async Task LoadRisksAsync()
    {
        var risks = await _riskAnalysis.GetActiveRisksAsync();

        ActiveRisks.Clear();
        foreach (var risk in risks)
        {
            ActiveRisks.Add(risk);
        }

        HasRisks = ActiveRisks.Count > 0;
    }
}
