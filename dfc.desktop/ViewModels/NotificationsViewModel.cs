using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class NotificationsViewModel : ViewModelBase
{
    private readonly ITeamNotificationRepository _notificationRepository;
    private readonly ILogger<NotificationsViewModel>? _logger;
    private readonly Guid _currentUserId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Mock user
    private readonly Action? _onNotificationRead;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<NotificationItemViewModel> _notifications = new();

    [ObservableProperty]
    private int _unreadCount;

    public NotificationsViewModel(ITeamNotificationRepository notificationRepository, Action? onNotificationRead = null, ILogger<NotificationsViewModel>? logger = null)
    {
        _notificationRepository = notificationRepository;
        _onNotificationRead = onNotificationRead;
        _logger = logger;
    }

    public async Task LoadNotificationsAsync()
    {
        try
        {
            IsLoading = true;

            var notifications = await _notificationRepository.GetByUserAsync(_currentUserId);

            Notifications.Clear();
            foreach (var notification in notifications.OrderByDescending(n => n.CreatedAt))
            {
                Notifications.Add(new NotificationItemViewModel(notification));
            }

            UnreadCount = Notifications.Count(n => !n.IsRead);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading notifications");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task MarkAsRead(NotificationItemViewModel? notification)
    {
        if (notification == null) return;

        try
        {
            notification.Notification.IsRead = true;
            notification.Notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification.Notification);

            notification.IsRead = true;
            UnreadCount = Notifications.Count(n => !n.IsRead);

            _onNotificationRead?.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error marking notification as read");
        }
    }

    [RelayCommand]
    private async Task MarkAllRead()
    {
        try
        {
            foreach (var notification in Notifications.Where(n => !n.IsRead))
            {
                notification.Notification.IsRead = true;
                notification.Notification.ReadAt = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(notification.Notification);
                notification.IsRead = true;
            }

            UnreadCount = 0;
            _onNotificationRead?.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error marking all as read");
        }
    }

    [RelayCommand]
    private async Task DeleteNotification(NotificationItemViewModel? notification)
    {
        if (notification == null) return;

        try
        {
            await _notificationRepository.DeleteAsync(notification.Notification.Id);
            Notifications.Remove(notification);

            if (!notification.IsRead)
            {
                UnreadCount--;
                _onNotificationRead?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting notification");
        }
    }
}

public partial class NotificationItemViewModel : ObservableObject
{
    public TeamNotification Notification { get; }

    [ObservableProperty]
    private bool _isRead;

    public NotificationItemViewModel(TeamNotification notification)
    {
        Notification = notification;
        _isRead = notification.IsRead;
    }

    public string Title => Notification.Title;
    public string Message => Notification.Message;

    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.UtcNow - Notification.CreatedAt;
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            return Notification.CreatedAt.ToString("MMM d");
        }
    }

    public string BackgroundColor => IsRead ? "Transparent" : "#F5F9FF";

    public string PriorityColor => Notification.Priority switch
    {
        NotificationPriority.Urgent => "#EF5350",
        NotificationPriority.High => "#FF9800",
        NotificationPriority.Normal => "#2196F3",
        NotificationPriority.Low => "#9E9E9E",
        _ => "#2196F3"
    };
}
