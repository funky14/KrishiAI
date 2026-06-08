using SQLite;

namespace KrishiAI.App.Models.Risk;

/// <summary>
/// Weather risk detection and analysis
/// </summary>
[Table("WeatherRisks")]
public class WeatherRisk
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Type of risk</summary>
    public RiskType RiskType { get; set; }

    /// <summary>Severity level</summary>
    public RiskLevel RiskLevel { get; set; }

    /// <summary>When the risk was detected</summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the risk is expected to occur</summary>
    public DateTime ExpectedAt { get; set; }

    /// <summary>When the risk period ends</summary>
    public DateTime EndsAt { get; set; }

    /// <summary>Location latitude</summary>
    public double Latitude { get; set; }

    /// <summary>Location longitude</summary>
    public double Longitude { get; set; }

    /// <summary>Location name</summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>Risk title</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Risk description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Recommended action</summary>
    public string RecommendedAction { get; set; } = string.Empty;

    /// <summary>Weather parameter value that triggered the risk</summary>
    public double TriggerValue { get; set; }

    /// <summary>Threshold value that was exceeded</summary>
    public double ThresholdValue { get; set; }

    /// <summary>Parameter unit (e.g., "°C", "mm", "km/h")</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Whether user has acknowledged this risk</summary>
    public bool IsAcknowledged { get; set; }

    /// <summary>Whether notification was sent</summary>
    public bool NotificationSent { get; set; }

    /// <summary>Whether risk is still active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Impact on crops</summary>
    public string CropImpact { get; set; } = string.Empty;

    /// <summary>Confidence score (0-100)</summary>
    public double ConfidenceScore { get; set; } = 85.0;

    // UI helpers (ignored by SQLite)
    [SQLite.Ignore]
    public string Icon
    {
        get
        {
            return RiskLevel switch
            {
                RiskLevel.Critical => "❗",
                RiskLevel.High => "⚠️",
                RiskLevel.Moderate => "⚠",
                _ => "ℹ️",
            };
        }
    }

    [SQLite.Ignore]
    public int DaysLeft
    {
        get
        {
            try
            {
                var days = (EndsAt.Date - DateTime.UtcNow.Date).Days;
                return Math.Max(0, days);
            }
            catch { return 0; }
        }
    }
}
