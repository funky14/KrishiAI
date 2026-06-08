using KrishiAI.App.Models.Risk;
using System.Diagnostics;

namespace KrishiAI.App.Services.Notification;

/// <summary>
/// Platform-agnostic notification service using MAUI essentials
/// For advanced features, integrate Plugin.LocalNotification
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IDatabaseService _database;
    private int _nextNotificationId = 1000;

    public NotificationService(IDatabaseService database)
    {
        _database = database;
    }

    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            // On Android 13+, notifications require runtime permission
            // For MAUI, notifications work by default on most platforms
            // Advanced: Add Plugin.LocalNotification for full notification permission management
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RequestPermissionAsync Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AreNotificationsEnabledAsync()
    {
        try
        {
            // Notifications are enabled by default in MAUI
            // For advanced permission checking, integrate Plugin.LocalNotification
            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }

    public async Task SendNotificationAsync(string title, string message, int notificationId = 0)
    {
        try
        {
            if (!await AreNotificationsEnabledAsync())
            {
                Debug.WriteLine("Notifications not enabled");
                return;
            }

            // For now, use simple toast/alert
            // In production, integrate Plugin.LocalNotification for full notification support
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlert(title, message, "OK");
            });

            Debug.WriteLine($"Notification sent: {title} - {message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SendNotificationAsync Error: {ex.Message}");
        }
    }

    public async Task ScheduleNotificationAsync(string title, string message, DateTime scheduledTime, int notificationId = 0)
    {
        try
        {
            // This is a simplified implementation
            // For production, use Plugin.LocalNotification or platform-specific implementations
            Debug.WriteLine($"Notification scheduled for {scheduledTime}: {title} - {message}");

            // Create alert in database for in-app display
            var alert = new WeatherAlert
            {
                Title = title,
                Message = message,
                ScheduledAt = scheduledTime,
                AlertType = "Scheduled",
                Priority = "Medium",
                Icon = "🔔"
            };

            await _database.SaveWeatherAlertAsync(alert);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ScheduleNotificationAsync Error: {ex.Message}");
        }
    }

    public async Task SendRiskAlertAsync(WeatherRisk risk)
    {
        try
        {
            var title = risk.Title;
            var message = $"{risk.Description}\n\n{risk.RecommendedAction}";
            var notificationId = _nextNotificationId++;

            await SendNotificationAsync(title, message, notificationId);

            // Create alert in database
            var alert = new WeatherAlert
            {
                WeatherRiskId = risk.Id,
                Title = title,
                Message = message,
                AlertType = "ActionRequired",
                Priority = risk.RiskLevel switch
                {
                    RiskLevel.Critical => "High",
                    RiskLevel.High => "High",
                    RiskLevel.Moderate => "Medium",
                    _ => "Low"
                },
                Icon = risk.RiskType.GetIcon(),
                BackgroundColor = risk.RiskLevel.GetColor(),
                NotificationSent = true,
                NotificationId = notificationId
            };

            await _database.SaveWeatherAlertAsync(alert);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SendRiskAlertAsync Error: {ex.Message}");
        }
    }

    public async Task SendIrrigationReminderAsync(string message, DateTime? scheduledTime = null)
    {
        try
        {
            var title = "💧 Irrigation Reminder";

            if (scheduledTime.HasValue && scheduledTime.Value > DateTime.Now)
            {
                await ScheduleNotificationAsync(title, message, scheduledTime.Value);
            }
            else
            {
                await SendNotificationAsync(title, message);
            }

            // Create alert in database
            var alert = new WeatherAlert
            {
                Title = title,
                Message = message,
                AlertType = "ActionRequired",
                Priority = "Medium",
                Icon = "💧",
                ScheduledAt = scheduledTime ?? DateTime.Now
            };

            await _database.SaveWeatherAlertAsync(alert);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SendIrrigationReminderAsync Error: {ex.Message}");
        }
    }

    public Task CancelNotificationAsync(int notificationId)
    {
        Debug.WriteLine($"Cancelling notification: {notificationId}");
        // Implement platform-specific cancellation
        return Task.CompletedTask;
    }

    public Task CancelAllNotificationsAsync()
    {
        Debug.WriteLine("Cancelling all notifications");
        // Implement platform-specific cancellation
        return Task.CompletedTask;
    }
}
