using SQLite;

namespace KrishiAI.App.Models.Irrigation;

/// <summary>
/// Crop information and configuration
/// </summary>
[Table("CropProfiles")]
public class CropInfo
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Crop type enum value</summary>
    public CropType CropType { get; set; }

    /// <summary>Custom crop name (for CropType.Custom)</summary>
    public string CustomCropName { get; set; } = string.Empty;

    /// <summary>Current growth stage</summary>
    public GrowthStage GrowthStage { get; set; }

    /// <summary>Current soil moisture percentage (0-100)</summary>
    public double SoilMoisturePercentage { get; set; }

    /// <summary>Field size in acres</summary>
    public double FieldSizeAcres { get; set; }

    /// <summary>Latitude of the field</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude of the field</summary>
    public double Longitude { get; set; }

    /// <summary>Location name</summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>Date when crop was planted</summary>
    public DateTime PlantedDate { get; set; }

    /// <summary>Expected harvest date</summary>
    public DateTime ExpectedHarvestDate { get; set; }

    /// <summary>Is this the active/primary crop</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Base water requirement in mm/day</summary>
    public double BaseWaterRequirement { get; set; }

    /// <summary>Minimum soil moisture threshold (%)</summary>
    public double MinSoilMoisture { get; set; } = 30.0;

    /// <summary>Optimal soil moisture range min (%)</summary>
    public double OptimalSoilMoistureMin { get; set; } = 60.0;

    /// <summary>Optimal soil moisture range max (%)</summary>
    public double OptimalSoilMoistureMax { get; set; } = 80.0;

    /// <summary>Maximum soil moisture threshold (%)</summary>
    public double MaxSoilMoisture { get; set; } = 100.0;

    /// <summary>Created timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last updated timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Get display name of the crop</summary>
    [Ignore]
    public string DisplayName => CropType == CropType.Custom && !string.IsNullOrEmpty(CustomCropName)
        ? CustomCropName
        : CropType.GetDisplayName();

    /// <summary>Get emoji for the crop</summary>
    [Ignore]
    public string Emoji => CropType.GetEmoji();
}
