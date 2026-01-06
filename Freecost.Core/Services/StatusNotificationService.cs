using System;

namespace Freecost.Core.Services;

/// <summary>
/// Implementation of status notification service.
/// This service acts as a message bus for UI notifications, allowing any part of the
/// application to trigger user-facing messages without direct coupling to the UI layer.
/// </summary>
public class StatusNotificationService : IStatusNotificationService
{
    /// <inheritdoc/>
    public event EventHandler<StatusNotificationEventArgs>? NotificationPosted;

    /// <inheritdoc/>
    public void ShowSuccess(string message, int durationMs = 3000)
    {
        PostNotification(NotificationLevel.Success, message, durationMs);
    }

    /// <inheritdoc/>
    public void ShowInfo(string message, int durationMs = 3000)
    {
        PostNotification(NotificationLevel.Info, message, durationMs);
    }

    /// <inheritdoc/>
    public void ShowWarning(string message, int durationMs = 5000)
    {
        PostNotification(NotificationLevel.Warning, message, durationMs);
    }

    /// <inheritdoc/>
    public void ShowError(string message, int durationMs = 0)
    {
        PostNotification(NotificationLevel.Error, message, durationMs);
    }

    /// <summary>
    /// Internal method to post a notification with the specified level and message.
    /// Raises the NotificationPosted event that UI components subscribe to.
    /// </summary>
    private void PostNotification(NotificationLevel level, string message, int durationMs)
    {
        NotificationPosted?.Invoke(this, new StatusNotificationEventArgs
        {
            Level = level,
            Message = message,
            DurationMs = durationMs,
            Timestamp = DateTime.Now
        });
    }
}
