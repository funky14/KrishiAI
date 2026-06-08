namespace KrishiAI.App.Models.Weather;

/// <summary>
/// Configurable weather risk thresholds
/// </summary>
public class WeatherThresholds
{
    /// <summary>Frost temperature threshold (°C)</summary>
    public double FrostTemperature { get; set; } = 3.0;

    /// <summary>Heatwave temperature threshold (°C)</summary>
    public double HeatwaveTemperature { get; set; } = 38.0;

    /// <summary>Heavy rain threshold (mm/24h)</summary>
    public double HeavyRainThreshold { get; set; } = 50.0;

    /// <summary>Drought days threshold (consecutive days without rain)</summary>
    public int DroughtDaysThreshold { get; set; } = 7;

    /// <summary>Strong wind threshold (km/h)</summary>
    public double StrongWindThreshold { get; set; } = 40.0;

    /// <summary>Minimum rain to be considered significant (mm)</summary>
    public double MinimumSignificantRain { get; set; } = 2.0;

    /// <summary>Critical high temperature (°C)</summary>
    public double CriticalHighTemperature { get; set; } = 42.0;

    /// <summary>Critical low temperature (°C)</summary>
    public double CriticalLowTemperature { get; set; } = 0.0;

    /// <summary>Extreme rain threshold (mm/24h)</summary>
    public double ExtremeRainThreshold { get; set; } = 100.0;

    /// <summary>Default thresholds</summary>
    public static WeatherThresholds Default => new()
    {
        FrostTemperature = 3.0,
        HeatwaveTemperature = 38.0,
        HeavyRainThreshold = 50.0,
        DroughtDaysThreshold = 7,
        StrongWindThreshold = 40.0,
        MinimumSignificantRain = 2.0,
        CriticalHighTemperature = 42.0,
        CriticalLowTemperature = 0.0,
        ExtremeRainThreshold = 100.0
    };
}
