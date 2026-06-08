namespace KrishiAI.App.Models.Irrigation;

/// <summary>
/// Supported crop types with their characteristics
/// </summary>
public enum CropType
{
    Rice,
    Wheat,
    Tomato,
    Potato,
    Onion,
    Cotton,
    Sugarcane,
    Maize,
    Custom
}

/// <summary>
/// Extension methods for CropType
/// </summary>
public static class CropTypeExtensions
{
    public static string GetDisplayName(this CropType cropType)
    {
        return cropType switch
        {
            CropType.Rice => "Rice",
            CropType.Wheat => "Wheat",
            CropType.Tomato => "Tomato",
            CropType.Potato => "Potato",
            CropType.Onion => "Onion",
            CropType.Cotton => "Cotton",
            CropType.Sugarcane => "Sugarcane",
            CropType.Maize => "Maize",
            CropType.Custom => "Custom Crop",
            _ => cropType.ToString()
        };
    }

    public static string GetDescription(this CropType cropType)
    {
        return cropType switch
        {
            CropType.Rice => "Water-intensive cereal crop",
            CropType.Wheat => "Drought-tolerant grain crop",
            CropType.Tomato => "High-value vegetable crop",
            CropType.Potato => "Root vegetable with moderate water needs",
            CropType.Onion => "Bulb crop with low water requirements",
            CropType.Cotton => "Fiber crop with moderate water needs",
            CropType.Sugarcane => "Water-intensive cash crop",
            CropType.Maize => "Versatile grain crop",
            CropType.Custom => "User-defined crop",
            _ => string.Empty
        };
    }

    public static string GetEmoji(this CropType cropType)
    {
        return cropType switch
        {
            CropType.Rice => "🌾",
            CropType.Wheat => "🌾",
            CropType.Tomato => "🍅",
            CropType.Potato => "🥔",
            CropType.Onion => "🧅",
            CropType.Cotton => "☁️",
            CropType.Sugarcane => "🎋",
            CropType.Maize => "🌽",
            CropType.Custom => "🌱",
            _ => "🌱"
        };
    }
}
