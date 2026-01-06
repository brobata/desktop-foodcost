using System;
using System.Net.Http;
using System.Net.Sockets;

namespace Freecost.Core.Helpers;

/// <summary>
/// Helper class for translating technical errors into user-friendly messages.
/// Provides context-aware error messages for common failure scenarios.
/// </summary>
public static class ErrorMessageHelper
{
    /// <summary>
    /// Converts a sync error into a user-friendly message with actionable guidance.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="context">Optional context about what was being synced</param>
    /// <returns>User-friendly error message</returns>
    public static string GetSyncErrorMessage(Exception exception, string? context = null)
    {
        var baseContext = string.IsNullOrEmpty(context) ? "syncing data" : context;

        return exception switch
        {
            HttpRequestException httpEx when httpEx.InnerException is SocketException =>
                "No internet connection available. Your changes are saved locally and will sync when you're back online.",

            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized =>
                "Your session has expired. Please sign out and sign in again to continue syncing.",

            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden =>
                "You don't have permission to sync this data. Contact your administrator if you believe this is an error.",

            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.NotFound =>
                $"The requested data was not found. It may have been deleted from the server.",

            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests =>
                "Too many sync attempts. Please wait a few minutes before trying again.",

            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable =>
                "The sync service is temporarily unavailable. Please try again in a few minutes.",

            TimeoutException =>
                "Sync timed out. Check your internet connection and try again.",

            SocketException =>
                "Network connection failed. Check your internet connection and try again.",

            UnauthorizedAccessException =>
                "Access denied. Please sign out and sign in again.",

            _ when exception.Message.Contains("SSL") || exception.Message.Contains("certificate") =>
                "Secure connection failed. Check your system date and time settings.",

            _ when exception.Message.Contains("JSON") || exception.Message.Contains("deserialization") =>
                "Data format error. This may be a temporary issue - please try syncing again.",

            _ when exception.Message.Contains("quota") || exception.Message.Contains("rate limit") =>
                "Cloud API rate limit exceeded. Please try again in a few minutes.",

            _ when exception.Message.Contains("permission") && exception.Message.Contains("denied") =>
                "Cloud permission denied. Please check your account permissions.",

            _ => $"Sync failed while {baseContext}. Your changes are saved locally."
        };
    }

    /// <summary>
    /// Gets a user-friendly message for authentication errors.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>User-friendly error message</returns>
    public static string GetAuthenticationErrorMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized =>
                "Invalid email or password. Please try again.",

            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests =>
                "Too many login attempts. Please wait a few minutes before trying again.",

            HttpRequestException httpEx when httpEx.InnerException is SocketException =>
                "No internet connection. Please check your connection and try again.",

            TimeoutException =>
                "Login request timed out. Please check your internet connection and try again.",

            _ when exception.Message.Contains("EMAIL_NOT_FOUND") =>
                "No account found with this email address.",

            _ when exception.Message.Contains("INVALID_PASSWORD") =>
                "Incorrect password. Please try again.",

            _ when exception.Message.Contains("USER_DISABLED") =>
                "This account has been disabled. Contact your administrator.",

            _ when exception.Message.Contains("TOO_MANY_ATTEMPTS_TRY_LATER") =>
                "Too many failed login attempts. Please try again later.",

            _ when exception.Message.Contains("WEAK_PASSWORD") =>
                "Password is too weak. Please use a stronger password.",

            _ when exception.Message.Contains("EMAIL_EXISTS") =>
                "An account with this email already exists.",

            _ => "Authentication failed. Please check your credentials and try again."
        };
    }

    /// <summary>
    /// Gets a user-friendly message for database errors.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="operation">The operation that failed (e.g., "saving ingredient")</param>
    /// <returns>User-friendly error message</returns>
    public static string GetDatabaseErrorMessage(Exception exception, string operation)
    {
        return exception switch
        {
            InvalidOperationException when exception.Message.Contains("closed") =>
                $"Database connection lost while {operation}. Please try again.",

            InvalidOperationException when exception.Message.Contains("locked") =>
                $"Database is locked by another operation. Please wait a moment and try again.",

            System.Data.Common.DbException when exception.Message.Contains("UNIQUE constraint") =>
                $"This item already exists. Please use a different name.",

            System.Data.Common.DbException when exception.Message.Contains("FOREIGN KEY constraint") =>
                $"Cannot complete {operation} - this item is being used elsewhere.",

            System.Data.Common.DbException when exception.Message.Contains("NOT NULL constraint") =>
                $"Required information is missing. Please fill in all required fields.",

            UnauthorizedAccessException =>
                $"Permission denied while {operation}. Check file permissions.",

            _ when exception.Message.Contains("disk") || exception.Message.Contains("space") =>
                "Not enough disk space. Please free up some space and try again.",

            _ when exception.Message.Contains("corrupt") =>
                "Database file may be corrupted. Please contact support.",

            _ => $"Database error while {operation}. Please try again."
        };
    }

    /// <summary>
    /// Gets a user-friendly message for network/connectivity errors.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>User-friendly error message</returns>
    public static string GetNetworkErrorMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException httpEx when httpEx.InnerException is SocketException =>
                "No internet connection. Please check your network settings.",

            SocketException socketEx when socketEx.SocketErrorCode == SocketError.HostNotFound =>
                "Cannot reach server. Check your internet connection.",

            SocketException socketEx when socketEx.SocketErrorCode == SocketError.TimedOut =>
                "Connection timed out. Server may be busy - please try again.",

            SocketException socketEx when socketEx.SocketErrorCode == SocketError.ConnectionRefused =>
                "Server connection refused. The service may be temporarily down.",

            TimeoutException =>
                "Request timed out. Check your internet connection and try again.",

            _ when exception.Message.Contains("DNS") =>
                "Cannot resolve server address. Check your DNS settings.",

            _ when exception.Message.Contains("SSL") || exception.Message.Contains("TLS") =>
                "Secure connection failed. Check your system date and time.",

            _ => "Network error. Please check your internet connection and try again."
        };
    }

    /// <summary>
    /// Gets a short, user-friendly summary of an error (suitable for status bar).
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>Brief error summary (max ~50 characters)</returns>
    public static string GetShortErrorMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException httpEx when httpEx.InnerException is SocketException => "No internet connection",
            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized => "Session expired",
            HttpRequestException httpEx when httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden => "Permission denied",
            TimeoutException => "Request timed out",
            SocketException => "Network error",
            UnauthorizedAccessException => "Access denied",
            _ when exception.Message.Contains("quota") || exception.Message.Contains("rate limit") => "API rate limit exceeded",
            _ => "Operation failed"
        };
    }

    /// <summary>
    /// Determines if an error is network-related and likely transient.
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if error is likely network-related and retryable</returns>
    public static bool IsNetworkError(Exception exception)
    {
        return exception is HttpRequestException httpEx && httpEx.InnerException is SocketException
            || exception is SocketException
            || exception is TimeoutException
            || exception.Message.Contains("network", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if an error is authentication-related.
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if error is authentication-related</returns>
    public static bool IsAuthenticationError(Exception exception)
    {
        return exception is HttpRequestException httpEx && (
                httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden)
            || exception is UnauthorizedAccessException
            || exception.Message.Contains("auth", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("forbidden", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if an error is retryable (temporary failure).
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if operation should be retried</returns>
    public static bool IsRetryable(Exception exception)
    {
        return exception is HttpRequestException httpEx && (
                httpEx.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                httpEx.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                httpEx.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            || exception is TimeoutException
            || exception is SocketException
            || exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("temporary", StringComparison.OrdinalIgnoreCase);
    }
}
