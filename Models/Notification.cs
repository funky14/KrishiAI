namespace KrishiAI.App.Models;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string Icon => Type switch
    {
        NotificationType.Alert => "⚠️",
        NotificationType.Reminder => "🔔",
        NotificationType.Tip => "💡",
        _ => "ℹ️"
    };

    public Color IconColor => Type switch
    {
        NotificationType.Alert => Colors.Red,
        NotificationType.Reminder => Colors.Orange,
        NotificationType.Tip => Colors.Yellow,
        _ => Colors.Blue
    };
}

public enum NotificationType
{
    Alert,
    Reminder,
    Tip,
    Info
}
