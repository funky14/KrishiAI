using SQLite;

namespace KrishiAI.App.Models.Weather;

/// <summary>
/// Complete weather forecast data including current, hourly, and daily forecasts
/// </summary>
[Table("WeatherForecasts")]
public class WeatherForecast
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Latitude of the location</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude of the location</summary>
    public double Longitude { get; set; }

    /// <summary>Location name (city, village)</summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>Timestamp when forecast was fetched</summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Timestamp when forecast expires (24 hours)</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);

    /// <summary>Current weather conditions</summary>
    [Ignore]
    public CurrentWeather? Current { get; set; }

    /// <summary>Hourly forecast for next 24 hours</summary>
    [Ignore]
    public List<HourlyForecast> HourlyForecasts { get; set; } = new();

    /// <summary>Daily forecast for next 7 days</summary>
    [Ignore]
    public List<DailyForecast> DailyForecasts { get; set; } = new();

    /// <summary>Whether this data is from cache (offline mode)</summary>
    [Ignore]
    public bool IsFromCache { get; set; }
}
