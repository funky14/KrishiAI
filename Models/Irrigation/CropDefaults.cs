namespace KrishiAI.App.Models.Irrigation;

/// <summary>
/// Default crop water requirements and characteristics
/// </summary>
public static class CropDefaults
{
    /// <summary>
    /// Get default water requirement for crop type (mm/day)
    /// </summary>
    public static double GetBaseWaterRequirement(CropType cropType)
    {
        return cropType switch
        {
            CropType.Rice => 7.5,           // High water requirement
            CropType.Wheat => 4.0,          // Moderate-low
            CropType.Tomato => 5.0,         // Moderate
            CropType.Potato => 4.5,         // Moderate
            CropType.Onion => 3.5,          // Low-moderate
            CropType.Cotton => 5.5,         // Moderate-high
            CropType.Sugarcane => 7.0,      // High
            CropType.Maize => 5.5,          // Moderate-high
            CropType.Custom => 5.0,         // Default moderate
            _ => 5.0
        };
    }

    /// <summary>
    /// Get optimal soil moisture range for crop
    /// </summary>
    public static (double min, double max) GetOptimalSoilMoisture(CropType cropType)
    {
        return cropType switch
        {
            CropType.Rice => (70, 90),          // Prefers wet soil
            CropType.Wheat => (50, 70),         // Moderate moisture
            CropType.Tomato => (60, 80),        // Consistent moisture
            CropType.Potato => (55, 75),        // Moderate moisture
            CropType.Onion => (45, 65),         // Lower moisture
            CropType.Cotton => (55, 75),        // Moderate moisture
            CropType.Sugarcane => (65, 85),     // High moisture
            CropType.Maize => (60, 80),         // Good moisture
            CropType.Custom => (60, 80),        // Default
            _ => (60, 80)
        };
    }

    /// <summary>
    /// Get crop growth duration in days
    /// </summary>
    public static int GetGrowthDurationDays(CropType cropType)
    {
        return cropType switch
        {
            CropType.Rice => 120,
            CropType.Wheat => 110,
            CropType.Tomato => 90,
            CropType.Potato => 100,
            CropType.Onion => 120,
            CropType.Cotton => 180,
            CropType.Sugarcane => 365,
            CropType.Maize => 100,
            CropType.Custom => 100,
            _ => 100
        };
    }

    /// <summary>
    /// Get critical temperature thresholds for crop (min, max) in Celsius
    /// </summary>
    public static (double min, double max) GetTemperatureThresholds(CropType cropType)
    {
        return cropType switch
        {
            CropType.Rice => (15, 37),
            CropType.Wheat => (3, 32),
            CropType.Tomato => (10, 35),
            CropType.Potato => (7, 30),
            CropType.Onion => (7, 30),
            CropType.Cotton => (15, 40),
            CropType.Sugarcane => (20, 38),
            CropType.Maize => (10, 38),
            CropType.Custom => (10, 35),
            _ => (10, 35)
        };
    }

    /// <summary>
    /// Initialize crop profile with defaults
    /// </summary>
    public static CropInfo CreateDefaultProfile(CropType cropType, string locationName = "", double latitude = 0, double longitude = 0)
    {
        var (minMoisture, maxMoisture) = GetOptimalSoilMoisture(cropType);
        var growthDays = GetGrowthDurationDays(cropType);

        return new CropInfo
        {
            CropType = cropType,
            GrowthStage = GrowthStage.Seedling,
            SoilMoisturePercentage = 70.0,
            FieldSizeAcres = 1.0,
            Latitude = latitude,
            Longitude = longitude,
            LocationName = locationName,
            PlantedDate = DateTime.Today,
            ExpectedHarvestDate = DateTime.Today.AddDays(growthDays),
            IsActive = true,
            BaseWaterRequirement = GetBaseWaterRequirement(cropType),
            MinSoilMoisture = 30.0,
            OptimalSoilMoistureMin = minMoisture,
            OptimalSoilMoistureMax = maxMoisture,
            MaxSoilMoisture = 100.0
        };
    }
}
