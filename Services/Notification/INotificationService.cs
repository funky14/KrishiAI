using KrishiAI.App.Models.Risk;

namespace KrishiAI.App.Services.Notification;

/// <summary>
/// Local notification service for weather and irrigation alerts
/// </summary>
public interface INotificationService
{
    /// <summary>Request notification permissions</summary>
    Task<bool> RequestPermissionAsync();

    /// <summary>Check if notifications are enabled</summary>
    Task<bool> AreNotificationsEnabledAsync();

    /// <summary>Send immediate notification</summary>
    Task SendNotificationAsync(string title, string message, int notificationId = 0);

    /// <summary>Schedule notification for later</summary>
    Task ScheduleNotificationAsync(string title, string message, DateTime scheduledTime, int notificationId = 0);

    /// <summary>Send risk alert notification</summary>
    Task SendRiskAlertAsync(WeatherRisk risk);

    /// <summary>Send irrigation reminder</summary>
    Task SendIrrigationReminderAsync(string message, DateTime? scheduledTime = null);

    /// <summary>Cancel specific notification</summary>
    Task CancelNotificationAsync(int notificationId);

    /// <summary>Cancel all pending notifications</summary>
    Task CancelAllNotificationsAsync();
}
