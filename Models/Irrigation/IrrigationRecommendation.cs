using SQLite;

namespace KrishiAI.App.Models.Irrigation;

/// <summary>
/// AI-generated irrigation recommendation
/// </summary>
[Table("IrrigationRecommendations")]
public class IrrigationRecommendation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Related crop profile ID</summary>
    [Indexed]
    public int CropProfileId { get; set; }

    /// <summary>Timestamp when recommendation was generated</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Should irrigate now</summary>
    public bool ShouldIrrigate { get; set; }

    /// <summary>Recommended irrigation duration in hours</summary>
    public double IrrigationDurationHours { get; set; }

    /// <summary>Recommended irrigation amount in mm</summary>
    public double IrrigationAmountMm { get; set; }

    /// <summary>Best time to irrigate (e.g., "Tomorrow morning", "This evening")</summary>
    public string BestIrrigationTime { get; set; } = string.Empty;

    /// <summary>Specific time to irrigate</summary>
    public DateTime? ScheduledTime { get; set; }

    /// <summary>Primary recommendation message</summary>
    public string RecommendationMessage { get; set; } = string.Empty;

    /// <summary>Detailed explanation</summary>
    public string DetailedExplanation { get; set; } = string.Empty;

    /// <summary>Reason for recommendation</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Current soil moisture at time of recommendation</summary>
    public double CurrentSoilMoisture { get; set; }

    /// <summary>Target soil moisture after irrigation</summary>
    public double TargetSoilMoisture { get; set; }

    /// <summary>Expected rainfall in next 24 hours (mm)</summary>
    public double ExpectedRainfall24h { get; set; }

    /// <summary>Current temperature (Celsius)</summary>
    public double CurrentTemperature { get; set; }

    /// <summary>Weather condition at time of recommendation</summary>
    public string WeatherCondition { get; set; } = string.Empty;

    /// <summary>Priority level (Low, Medium, High, Critical)</summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>Water saving percentage compared to standard irrigation</summary>
    public double WaterSavingPercentage { get; set; }

    /// <summary>Confidence score (0-100)</summary>
    public double ConfidenceScore { get; set; }

    /// <summary>Whether user acted on this recommendation</summary>
    public bool UserActioned { get; set; }

    /// <summary>User feedback (if any)</summary>
    public string? UserFeedback { get; set; }

    /// <summary>Validity period - expires after this time</summary>
    public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddHours(12);
}
