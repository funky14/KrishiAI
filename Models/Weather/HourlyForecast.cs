using SQLite;

namespace KrishiAI.App.Models.Weather;

/// <summary>
/// Hourly weather forecast
/// </summary>
[Table("HourlyForecasts")]
public class HourlyForecast
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Foreign key to WeatherForecast</summary>
    [Indexed]
    public int ForecastId { get; set; }

    /// <summary>Forecast timestamp</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Temperature in Celsius</summary>
    public double Temperature { get; set; }

    /// <summary>Feels like temperature in Celsius</summary>
    public double FeelsLike { get; set; }

    /// <summary>Humidity percentage (0-100)</summary>
    public int Humidity { get; set; }

    /// <summary>Wind speed in km/h</summary>
    public double WindSpeed { get; set; }

    /// <summary>Rainfall in mm</summary>
    public double Rainfall { get; set; }

    /// <summary>Probability of precipitation (0-100)</summary>
    public int PrecipitationProbability { get; set; }

    /// <summary>Cloud cover percentage (0-100)</summary>
    public int CloudCover { get; set; }

    /// <summary>Weather condition code</summary>
    public int WeatherCode { get; set; }

    /// <summary>Weather description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Weather icon identifier</summary>
    public string Icon { get; set; } = string.Empty;
}
