namespace KrishiAI.App.Services.Weather;

/// <summary>
/// WMO Weather interpretation codes (WW)
/// Source: https://open-meteo.com/en/docs
/// </summary>
public static class WeatherCodeHelper
{
    public static string GetDescription(int code)
    {
        return code switch
        {
            0 => "Clear sky",
            1 => "Mainly clear",
            2 => "Partly cloudy",
            3 => "Overcast",
            45 => "Foggy",
            48 => "Depositing rime fog",
            51 => "Light drizzle",
            53 => "Moderate drizzle",
            55 => "Dense drizzle",
            56 => "Light freezing drizzle",
            57 => "Dense freezing drizzle",
            61 => "Slight rain",
            63 => "Moderate rain",
            65 => "Heavy rain",
            66 => "Light freezing rain",
            67 => "Heavy freezing rain",
            71 => "Slight snow",
            73 => "Moderate snow",
            75 => "Heavy snow",
            77 => "Snow grains",
            80 => "Slight rain showers",
            81 => "Moderate rain showers",
            82 => "Violent rain showers",
            85 => "Slight snow showers",
            86 => "Heavy snow showers",
            95 => "Thunderstorm",
            96 => "Thunderstorm with slight hail",
            99 => "Thunderstorm with heavy hail",
            _ => "Unknown"
        };
    }

    public static string GetIcon(int code)
    {
        return code switch
        {
            0 => "☀️",           // Clear
            1 or 2 => "🌤️",    // Partly cloudy
            3 => "☁️",           // Overcast
            45 or 48 => "🌫️",  // Fog
            51 or 53 or 55 or 56 or 57 => "🌧️",  // Drizzle
            61 or 63 or 80 or 81 => "🌧️",        // Rain
            65 or 82 => "⛈️",   // Heavy rain
            66 or 67 => "🧊",   // Freezing rain
            71 or 73 or 75 or 77 or 85 or 86 => "🌨️",  // Snow
            95 or 96 or 99 => "⛈️",  // Thunderstorm
            _ => "🌡️"
        };
    }

    public static bool IsRainy(int code)
    {
        return code is >= 51 and <= 67 or >= 80 and <= 82 or >= 95 and <= 99;
    }

    public static bool IsSevere(int code)
    {
        return code is 65 or 67 or 75 or 82 or 86 or >= 95;
    }
}
