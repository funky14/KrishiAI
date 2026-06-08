using KrishiAI.App.Models.Weather;

namespace KrishiAI.App.Services.Weather;

/// <summary>
/// Weather forecast service interface
/// </summary>
public interface IWeatherService
{
    /// <summary>Get current weather and forecast for location</summary>
    Task<WeatherForecast?> GetWeatherForecastAsync(double latitude, double longitude, string locationName = "");

    /// <summary>Get weather forecast using device's current location</summary>
    Task<WeatherForecast?> GetCurrentLocationWeatherAsync();

    /// <summary>Get cached weather forecast (offline mode)</summary>
    Task<WeatherForecast?> GetCachedWeatherAsync(double latitude, double longitude);

    /// <summary>Get cached weather for most recent location</summary>
    Task<WeatherForecast?> GetLatestCachedWeatherAsync();

    /// <summary>Refresh weather data</summary>
    Task<WeatherForecast?> RefreshWeatherAsync(double latitude, double longitude, string locationName = "");

    /// <summary>Check if weather data is available (online or cached)</summary>
    Task<bool> IsWeatherDataAvailableAsync();

    /// <summary>Clean up expired cache</summary>
    Task CleanupExpiredCacheAsync();
}
