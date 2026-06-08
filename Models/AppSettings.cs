using KrishiAI.App.Models.Weather;

namespace KrishiAI.App.Models;

public class AppSettings
{
    public string PreferredLanguage { get; set; } = "en-US";
    public bool SaveHistory { get; set; } = true;
    public bool CacheImages { get; set; } = true;
    public int MaxHistoryItems { get; set; } = 100;
    public bool AutoPlayResponses { get; set; } = true;
    public string Theme { get; set; } = "Light";

    // Weather & Irrigation Settings
    public bool EnableWeatherNotifications { get; set; } = true;
    public bool EnableIrrigationReminders { get; set; } = true;
    public bool UseHyperLocalWeather { get; set; } = true;
    public int WeatherCacheDurationHours { get; set; } = 24;
    public WeatherThresholds WeatherThresholds { get; set; } = WeatherThresholds.Default;
}
