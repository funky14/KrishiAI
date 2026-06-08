using System.Text.Json;
using System.Diagnostics;

namespace KrishiAI.App.Services.Weather;

/// <summary>
/// Open-Meteo API client for free weather data
/// Documentation: https://open-meteo.com/en/docs
/// </summary>
public class OpenMeteoClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.open-meteo.com/v1";

    public OpenMeteoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Get weather forecast from Open-Meteo API
    /// </summary>
    public async Task<OpenMeteoResponse?> GetForecastAsync(double latitude, double longitude)
    {
        try
        {
            var url = BuildForecastUrl(latitude, longitude);
            Debug.WriteLine($"Open-Meteo Request: {url}");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<OpenMeteoResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Open-Meteo API Error: {ex.Message}");
            return null;
        }
    }

    private string BuildForecastUrl(double latitude, double longitude)
    {
        // Request comprehensive weather data
        return $"{BaseUrl}/forecast?" +
               $"latitude={latitude:F4}&longitude={longitude:F4}" +
               $"&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,rain," +
               $"weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,uv_index" +
               $"&hourly=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation_probability," +
               $"precipitation,rain,weather_code,cloud_cover,wind_speed_10m" +
               $"&daily=weather_code,temperature_2m_max,temperature_2m_min,sunrise,sunset,uv_index_max," +
               $"precipitation_sum,rain_sum,precipitation_probability_max,wind_speed_10m_max" +
               $"&timezone=auto" +
               $"&forecast_days=7";
    }

    /// <summary>
    /// Geocoding - Get location name from coordinates (reverse geocoding)
    /// Uses Nominatim OpenStreetMap API for reverse geocoding
    /// </summary>
    public async Task<string> GetLocationNameAsync(double latitude, double longitude)
    {
        try
        {
            // Use Nominatim (OpenStreetMap) for reverse geocoding
            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude:F6}&lon={longitude:F6}&zoom=10";

            // Nominatim requires a User-Agent header
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "KrishiAI-MAUI-App/1.0");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            if (data.TryGetProperty("address", out var address))
            {
                // Try to build: City, State, Country
                var city = address.TryGetProperty("city", out var c) ? c.GetString() : 
                          address.TryGetProperty("town", out var t) ? t.GetString() :
                          address.TryGetProperty("village", out var v) ? v.GetString() :
                          address.TryGetProperty("county", out var co) ? co.GetString() : null;

                var state = address.TryGetProperty("state", out var s) ? s.GetString() : null;
                var country = address.TryGetProperty("country", out var cn) ? cn.GetString() : null;

                // Build location string
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(city)) parts.Add(city);
                if (!string.IsNullOrEmpty(state)) parts.Add(state);
                if (!string.IsNullOrEmpty(country)) parts.Add(country);

                if (parts.Count > 0)
                    return string.Join(", ", parts);
            }

            // Fallback to coordinates
            return $"{latitude:F2}, {longitude:F2}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Reverse geocoding error: {ex.Message}");
            return $"{latitude:F2}, {longitude:F2}";
        }
    }
}

/// <summary>
/// Open-Meteo API response structure
/// </summary>
public class OpenMeteoResponse
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Timezone { get; set; }
    public CurrentWeatherData? Current { get; set; }
    public HourlyWeatherData? Hourly { get; set; }
    public DailyWeatherData? Daily { get; set; }
}

public class CurrentWeatherData
{
    public string? Time { get; set; }
    public double Temperature_2m { get; set; }
    public double Relative_Humidity_2m { get; set; }
    public double Apparent_Temperature { get; set; }
    public double Precipitation { get; set; }
    public double Rain { get; set; }
    public int Weather_Code { get; set; }
    public int Cloud_Cover { get; set; }
    public double Pressure_Msl { get; set; }
    public double Surface_Pressure { get; set; }
    public double Wind_Speed_10m { get; set; }
    public int Wind_Direction_10m { get; set; }
    public double Uv_Index { get; set; }
}

public class HourlyWeatherData
{
    public List<string>? Time { get; set; }
    public List<double>? Temperature_2m { get; set; }
    public List<int>? Relative_Humidity_2m { get; set; }
    public List<double>? Apparent_Temperature { get; set; }
    public List<int>? Precipitation_Probability { get; set; }
    public List<double>? Precipitation { get; set; }
    public List<double>? Rain { get; set; }
    public List<int>? Weather_Code { get; set; }
    public List<int>? Cloud_Cover { get; set; }
    public List<double>? Wind_Speed_10m { get; set; }
}

public class DailyWeatherData
{
    public List<string>? Time { get; set; }
    public List<int>? Weather_Code { get; set; }
    public List<double>? Temperature_2m_Max { get; set; }
    public List<double>? Temperature_2m_Min { get; set; }
    public List<string>? Sunrise { get; set; }
    public List<string>? Sunset { get; set; }
    public List<double>? Uv_Index_Max { get; set; }
    public List<double>? Precipitation_Sum { get; set; }
    public List<double>? Rain_Sum { get; set; }
    public List<int>? Precipitation_Probability_Max { get; set; }
    public List<double>? Wind_Speed_10m_Max { get; set; }
}
