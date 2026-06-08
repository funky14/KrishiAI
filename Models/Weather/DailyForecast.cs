using SQLite;

namespace KrishiAI.App.Models.Weather;

/// <summary>
/// Daily weather forecast
/// </summary>
[Table("DailyForecasts")]
public class DailyForecast
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Foreign key to WeatherForecast</summary>
    [Indexed]
    public int ForecastId { get; set; }

    /// <summary>Date of the forecast</summary>
    public DateTime Date { get; set; }

    /// <summary>Maximum temperature in Celsius</summary>
    public double TemperatureMax { get; set; }

    /// <summary>Minimum temperature in Celsius</summary>
    public double TemperatureMin { get; set; }

    /// <summary>Sunrise time</summary>
    public DateTime Sunrise { get; set; }

    /// <summary>Sunset time</summary>
    public DateTime Sunset { get; set; }

    /// <summary>Total rainfall in mm</summary>
    public double Rainfall { get; set; }

    /// <summary>Maximum wind speed in km/h</summary>
    public double WindSpeedMax { get; set; }

    /// <summary>Average humidity percentage</summary>
    public int HumidityAvg { get; set; }

    /// <summary>Probability of precipitation (0-100)</summary>
    public int PrecipitationProbability { get; set; }

    /// <summary>Maximum UV index</summary>
    public double UVIndexMax { get; set; }

    /// <summary>Weather condition code</summary>
    public int WeatherCode { get; set; }

    /// <summary>Weather description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Weather icon identifier</summary>
    public string Icon { get; set; } = string.Empty;
}
