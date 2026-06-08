namespace KrishiAI.App.Models.Risk;

/// <summary>
/// Risk severity levels
/// </summary>
public enum RiskLevel
{
    None,
    Low,
    Moderate,
    High,
    Critical
}

/// <summary>
/// Types of weather risks
/// </summary>
public enum RiskType
{
    Frost,
    Heatwave,
    HeavyRain,
    Drought,
    StrongWind,
    Storm
}

/// <summary>
/// Extension methods for risk enums
/// </summary>
public static class RiskExtensions
{
    public static string GetDisplayName(this RiskLevel level)
    {
        return level switch
        {
            RiskLevel.None => "No Risk",
            RiskLevel.Low => "Low Risk",
            RiskLevel.Moderate => "Moderate Risk",
            RiskLevel.High => "High Risk",
            RiskLevel.Critical => "Critical Risk",
            _ => level.ToString()
        };
    }

    public static string GetColor(this RiskLevel level)
    {
        return level switch
        {
            RiskLevel.None => "#4CAF50",      // Green
            RiskLevel.Low => "#8BC34A",       // Light Green
            RiskLevel.Moderate => "#FFC107",  // Amber
            RiskLevel.High => "#FF9800",      // Orange
            RiskLevel.Critical => "#F44336",  // Red
            _ => "#9E9E9E"                    // Gray
        };
    }

    public static string GetEmoji(this RiskLevel level)
    {
        return level switch
        {
            RiskLevel.None => "✅",
            RiskLevel.Low => "⚠️",
            RiskLevel.Moderate => "⚠️",
            RiskLevel.High => "🚨",
            RiskLevel.Critical => "🔴",
            _ => "ℹ️"
        };
    }

    public static string GetDisplayName(this RiskType type)
    {
        return type switch
        {
            RiskType.Frost => "Frost Risk",
            RiskType.Heatwave => "Heatwave Risk",
            RiskType.HeavyRain => "Heavy Rain Risk",
            RiskType.Drought => "Drought Risk",
            RiskType.StrongWind => "Strong Wind Risk",
            RiskType.Storm => "Storm Risk",
            _ => type.ToString()
        };
    }

    public static string GetIcon(this RiskType type)
    {
        return type switch
        {
            RiskType.Frost => "❄️",
            RiskType.Heatwave => "🌡️",
            RiskType.HeavyRain => "🌧️",
            RiskType.Drought => "🏜️",
            RiskType.StrongWind => "💨",
            RiskType.Storm => "⛈️",
            _ => "⚠️"
        };
    }
}
