using SQLite;

namespace KrishiAI.App.Models.Risk;

/// <summary>
/// Weather alert for notifications
/// </summary>
[Table("WeatherAlerts")]
public class WeatherAlert
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Related weather risk ID</summary>
    [Indexed]
    public int WeatherRiskId { get; set; }

    /// <summary>Alert type (Informational, ActionRequired)</summary>
    public string AlertType { get; set; } = "Informational";

    /// <summary>Alert title</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Alert message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>When alert was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When alert should be shown</summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>Alert priority (Low, Medium, High)</summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>Whether alert has been shown</summary>
    public bool IsShown { get; set; }

    /// <summary>Whether user has dismissed the alert</summary>
    public bool IsDismissed { get; set; }

    /// <summary>Action button text (optional)</summary>
    public string? ActionButtonText { get; set; }

    /// <summary>Action to perform when clicked</summary>
    public string? ActionRoute { get; set; }

    /// <summary>Icon to display</summary>
    public string Icon { get; set; } = "⚠️";

    /// <summary>Background color</summary>
    public string BackgroundColor { get; set; } = "#FFC107";

    /// <summary>Whether notification was sent to OS</summary>
    public bool NotificationSent { get; set; }

    /// <summary>Notification ID in OS</summary>
    public int? NotificationId { get; set; }
}
