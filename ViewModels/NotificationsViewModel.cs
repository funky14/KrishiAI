using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class NotificationsViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Notification> notifications = new();

    [ObservableProperty]
    private int notificationCount;

    public NotificationsViewModel()
    {
        Title = Resources.Strings.AppStrings.Notifications;
        LoadNotifications();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        Title = Resources.Strings.AppStrings.Notifications;
        System.Diagnostics.Debug.WriteLine("🌍 NotificationsViewModel: Language changed");
    }

    private void LoadNotifications()
    {
        // Sample notifications - in production, load from database or service
        Notifications = new ObservableCollection<Notification>
        {
            new Notification
            {
                Id = 1,
                Title = "Crop disease alert",
                Message = "Your paddy crop has detected moisture stress.",
                Type = NotificationType.Alert,
                CreatedAt = DateTime.Now.AddHours(-2),
                IsRead = false
            },
            new Notification
            {
                Id = 2,
                Title = "Morning reminder",
                Message = "Check your irrigation schedule before 10 AM.",
                Type = NotificationType.Reminder,
                CreatedAt = DateTime.Now.AddHours(-5),
                IsRead = false
            },
            new Notification
            {
                Id = 3,
                Title = "Tip of the day",
                Message = "Use natural compost for healthier leaves.",
                Type = NotificationType.Tip,
                CreatedAt = DateTime.Now.AddHours(-8),
                IsRead = true
            }
        };

        NotificationCount = Notifications.Count(n => !n.IsRead);
    }

    [RelayCommand]
    private void MarkAsRead(Notification notification)
    {
        notification.IsRead = true;
        NotificationCount = Notifications.Count(n => !n.IsRead);
    }

    [RelayCommand]
    private void ClearNotifications()
    {
        Notifications.Clear();
        NotificationCount = 0;
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.GoToAsync("..");
    }
}
