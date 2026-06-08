using KrishiAI.App.Models.Weather;
using System.Diagnostics;

namespace KrishiAI.App.Services.Weather;

/// <summary>
/// Weather service implementation with Open-Meteo API and offline caching
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly OpenMeteoClient _apiClient;
    private readonly IDatabaseService _database;
    private readonly IConnectivityService _connectivity;

    public WeatherService(
        HttpClient httpClient,
        IDatabaseService database,
        IConnectivityService connectivity)
    {
        _apiClient = new OpenMeteoClient(httpClient);
        _database = database;
        _connectivity = connectivity;
    }

    public async Task<WeatherForecast?> GetWeatherForecastAsync(double latitude, double longitude, string locationName = "")
    {
        try
        {
            // Check connectivity
            if (!_connectivity.IsConnected())
            {
                Debug.WriteLine("No internet connection. Using cached weather data.");
                return await GetCachedWeatherAsync(latitude, longitude);
            }

            // Try to fetch from API
            var apiResponse = await _apiClient.GetForecastAsync(latitude, longitude);
            if (apiResponse == null)
            {
                Debug.WriteLine("API request failed. Using cached data.");
                return await GetCachedWeatherAsync(latitude, longitude);
            }

            // Get location name if not provided
            if (string.IsNullOrEmpty(locationName))
            {
                locationName = await _apiClient.GetLocationNameAsync(latitude, longitude);
            }

            // Convert API response to our model
            var forecast = ConvertToWeatherForecast(apiResponse, locationName);

            // Cache the forecast
            await _database.SaveWeatherForecastAsync(forecast);

            return forecast;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetWeatherForecastAsync Error: {ex.Message}");
            return await GetCachedWeatherAsync(latitude, longitude);
        }
    }

    public async Task<WeatherForecast?> GetCurrentLocationWeatherAsync()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });
            }

            if (location == null)
            {
                Debug.WriteLine("Could not get device location");
                return await GetLatestCachedWeatherAsync();
            }

            // Let the API client perform reverse geocoding so we get City, State, Country format
            return await GetWeatherForecastAsync(
                location.Latitude,
                location.Longitude
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCurrentLocationWeatherAsync Error: {ex.Message}");
            return await GetLatestCachedWeatherAsync();
        }
    }

    public async Task<WeatherForecast?> GetCachedWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var cached = await _database.GetWeatherForecastAsync(latitude, longitude);
            if (cached != null)
            {
                cached.IsFromCache = true;
            }
            return cached;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCachedWeatherAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task<WeatherForecast?> GetLatestCachedWeatherAsync()
    {
        try
        {
            var cached = await _database.GetLatestWeatherForecastAsync();
            if (cached != null)
            {
                cached.IsFromCache = true;
            }
            return cached;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetLatestCachedWeatherAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task<WeatherForecast?> RefreshWeatherAsync(double latitude, double longitude, string locationName = "")
    {
        // Force refresh by calling API directly
        return await GetWeatherForecastAsync(latitude, longitude, locationName);
    }

    public async Task<bool> IsWeatherDataAvailableAsync()
    {
        var cached = await GetLatestCachedWeatherAsync();
        return cached != null || _connectivity.IsConnected();
    }

    public async Task CleanupExpiredCacheAsync()
    {
        try
        {
            await _database.DeleteExpiredWeatherForecastsAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CleanupExpiredCacheAsync Error: {ex.Message}");
        }
    }

    private WeatherForecast ConvertToWeatherForecast(OpenMeteoResponse apiData, string locationName)
    {
        var forecast = new WeatherForecast
        {
            Latitude = apiData.Latitude,
            Longitude = apiData.Longitude,
            LocationName = locationName,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        // Convert current weather
        if (apiData.Current != null)
        {
            forecast.Current = new CurrentWeather
            {
                Timestamp = DateTime.UtcNow,
                Temperature = apiData.Current.Temperature_2m,
                FeelsLike = apiData.Current.Apparent_Temperature,
                Humidity = (int)apiData.Current.Relative_Humidity_2m,
                Pressure = apiData.Current.Pressure_Msl,
                WindSpeed = apiData.Current.Wind_Speed_10m,
                WindDirection = apiData.Current.Wind_Direction_10m,
                CloudCover = apiData.Current.Cloud_Cover,
                UVIndex = apiData.Current.Uv_Index,
                Visibility = 10000, // Open-Meteo doesn't provide visibility
                Rainfall = apiData.Current.Rain,
                WeatherCode = apiData.Current.Weather_Code,
                Description = WeatherCodeHelper.GetDescription(apiData.Current.Weather_Code),
                Icon = WeatherCodeHelper.GetIcon(apiData.Current.Weather_Code),
                PrecipitationProbability = 0 // Not available in current data
            };
        }

        // Convert hourly forecasts (next 24 hours)
        if (apiData.Hourly != null && apiData.Hourly.Time != null)
        {
            for (int i = 0; i < Math.Min(24, apiData.Hourly.Time.Count); i++)
            {
                var hourly = new HourlyForecast
                {
                    Timestamp = DateTime.Parse(apiData.Hourly.Time[i]),
                    Temperature = apiData.Hourly.Temperature_2m?[i] ?? 0,
                    FeelsLike = apiData.Hourly.Apparent_Temperature?[i] ?? 0,
                    Humidity = apiData.Hourly.Relative_Humidity_2m?[i] ?? 0,
                    WindSpeed = apiData.Hourly.Wind_Speed_10m?[i] ?? 0,
                    Rainfall = apiData.Hourly.Rain?[i] ?? 0,
                    PrecipitationProbability = apiData.Hourly.Precipitation_Probability?[i] ?? 0,
                    CloudCover = apiData.Hourly.Cloud_Cover?[i] ?? 0,
                    WeatherCode = apiData.Hourly.Weather_Code?[i] ?? 0,
                    Description = WeatherCodeHelper.GetDescription(apiData.Hourly.Weather_Code?[i] ?? 0),
                    Icon = WeatherCodeHelper.GetIcon(apiData.Hourly.Weather_Code?[i] ?? 0)
                };
                forecast.HourlyForecasts.Add(hourly);
            }
        }

        // Convert daily forecasts (next 7 days)
        if (apiData.Daily != null && apiData.Daily.Time != null)
        {
            for (int i = 0; i < apiData.Daily.Time.Count; i++)
            {
                var daily = new DailyForecast
                {
                    Date = DateTime.Parse(apiData.Daily.Time[i]),
                    TemperatureMax = apiData.Daily.Temperature_2m_Max?[i] ?? 0,
                    TemperatureMin = apiData.Daily.Temperature_2m_Min?[i] ?? 0,
                    Sunrise = apiData.Daily.Sunrise != null && i < apiData.Daily.Sunrise.Count 
                        ? DateTime.Parse(apiData.Daily.Sunrise[i]) 
                        : DateTime.Today.AddHours(6),
                    Sunset = apiData.Daily.Sunset != null && i < apiData.Daily.Sunset.Count 
                        ? DateTime.Parse(apiData.Daily.Sunset[i]) 
                        : DateTime.Today.AddHours(18),
                    Rainfall = apiData.Daily.Rain_Sum?[i] ?? 0,
                    WindSpeedMax = apiData.Daily.Wind_Speed_10m_Max?[i] ?? 0,
                    HumidityAvg = 60, // Not provided by API, use default
                    PrecipitationProbability = apiData.Daily.Precipitation_Probability_Max?[i] ?? 0,
                    UVIndexMax = apiData.Daily.Uv_Index_Max?[i] ?? 0,
                    WeatherCode = apiData.Daily.Weather_Code?[i] ?? 0,
                    Description = WeatherCodeHelper.GetDescription(apiData.Daily.Weather_Code?[i] ?? 0),
                    Icon = WeatherCodeHelper.GetIcon(apiData.Daily.Weather_Code?[i] ?? 0)
                };
                forecast.DailyForecasts.Add(daily);
            }
        }

        return forecast;
    }
}
