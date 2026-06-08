using SQLite;

namespace KrishiAI.App.Models.Weather;

/// <summary>
/// Current weather conditions
/// </summary>
[Table("CurrentWeather")]
public class CurrentWeather
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Foreign key to WeatherForecast</summary>
    [Indexed]
    public int ForecastId { get; set; }

    /// <summary>Timestamp of the weather observation</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Temperature in Celsius</summary>
    public double Temperature { get; set; }

    /// <summary>Feels like temperature in Celsius</summary>
    public double FeelsLike { get; set; }

    /// <summary>Humidity percentage (0-100)</summary>
    public int Humidity { get; set; }

    /// <summary>Atmospheric pressure in hPa</summary>
    public double Pressure { get; set; }

    /// <summary>Wind speed in km/h</summary>
    public double WindSpeed { get; set; }

    /// <summary>Wind direction in degrees (0-360)</summary>
    public int WindDirection { get; set; }

    /// <summary>Cloud cover percentage (0-100)</summary>
    public int CloudCover { get; set; }

    /// <summary>UV index (0-11+)</summary>
    public double UVIndex { get; set; }

    /// <summary>Visibility in meters</summary>
    public double Visibility { get; set; }

    /// <summary>Rainfall in mm</summary>
    public double Rainfall { get; set; }

    /// <summary>Weather condition code (from API)</summary>
    public int WeatherCode { get; set; }

    /// <summary>Weather description (e.g., "Clear sky", "Light rain")</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Weather icon identifier</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>Probability of precipitation (0-100)</summary>
    public int PrecipitationProbability { get; set; }
}
