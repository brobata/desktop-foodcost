using System;

namespace Dfc.Core.Services;

/// <summary>
/// Defines the severity level of a status notification
/// </summary>
public enum NotificationLevel
{
    /// <summary>Operation completed successfully</summary>
    Success,

    /// <summary>Informational message for the user</summary>
    Info,

    /// <summary>Warning that requires user attention but is not critical</summary>
    Warning,

    /// <summary>Error that prevented an operation from completing</summary>
    Error
}

/// <summary>
/// Service for displaying status notifications (toasts) to the user.
/// Provides a centralized way to show success, info, warning, and error messages
/// across the application with consistent styling and behavior.
/// </summary>
public interface IStatusNotificationService
{
    /// <summary>
    /// Event raised when a notification needs to be displayed.
    /// UI components should subscribe to this event to show the notification.
    /// </summary>
    event EventHandler<StatusNotificationEventArgs>? NotificationPosted;

    /// <summary>
    /// Displays a success notification (green, checkmark icon).
    /// Used for operations that completed successfully.
    /// </summary>
    /// <param name="message">The success message to display</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismissing (default: 3000ms)</param>
    /// <example>
    /// ShowSuccess("Recipe saved successfully");
    /// </example>
    void ShowSuccess(string message, int durationMs = 3000);

    /// <summary>
    /// Displays an informational notification (blue, info icon).
    /// Used for neutral information or system messages.
    /// </summary>
    /// <param name="message">The informational message to display</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismissing (default: 3000ms)</param>
    /// <example>
    /// ShowInfo("Sync started in background");
    /// </example>
    void ShowInfo(string message, int durationMs = 3000);

    /// <summary>
    /// Displays a warning notification (yellow/orange, warning icon).
    /// Used for situations that require user attention but are not critical errors.
    /// </summary>
    /// <param name="message">The warning message to display</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismissing (default: 5000ms)</param>
    /// <example>
    /// ShowWarning("Some ingredients are missing pricing information");
    /// </example>
    void ShowWarning(string message, int durationMs = 5000);

    /// <summary>
    /// Displays an error notification (red, error icon).
    /// Used for operations that failed or critical issues requiring immediate attention.
    /// By default, error notifications stay visible until the user dismisses them.
    /// </summary>
    /// <param name="message">The error message to display</param>
    /// <param name="durationMs">Duration in milliseconds before auto-dismissing (default: 0 = stays until dismissed)</param>
    /// <example>
    /// ShowError("Failed to save recipe. Please try again.");
    /// </example>
    void ShowError(string message, int durationMs = 0); // 0 = stays until dismissed
}

/// <summary>
/// Event arguments for status notifications.
/// Contains all information needed to display a notification to the user.
/// </summary>
public class StatusNotificationEventArgs : EventArgs
{
    /// <summary>
    /// The severity level of the notification (Success, Info, Warning, Error)
    /// </summary>
    public NotificationLevel Level { get; set; }

    /// <summary>
    /// The message text to display to the user
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Duration in milliseconds before the notification auto-dismisses.
    /// A value of 0 means the notification stays visible until manually dismissed.
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// The timestamp when the notification was created
    /// </summary>
    public DateTime Timestamp { get; set; }
}
