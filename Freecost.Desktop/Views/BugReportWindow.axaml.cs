using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using Freecost.Core.Services;
using Microsoft.Extensions.Logging;

namespace Freecost.Desktop.Views;

public partial class BugReportWindow : Window
{
    private readonly IBugReportService _bugReportService;
    private readonly IStatusNotificationService _notificationService;
    private readonly ILogger<BugReportWindow>? _logger;

    // Parameterless constructor for Avalonia XAML
    public BugReportWindow() : this(null!, null!, null)
    {
    }

    public BugReportWindow(
        IBugReportService bugReportService,
        IStatusNotificationService notificationService,
        ILogger<BugReportWindow>? logger = null)
    {
        InitializeComponent();
        _bugReportService = bugReportService;
        _notificationService = notificationService;
        _logger = logger;

        // Focus the first text box when window opens
        Opened += (s, e) => WhatDoingTextBox?.Focus();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void OnSubmitClick(object? sender, RoutedEventArgs e)
    {
        var whatDoing = WhatDoingTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(whatDoing))
        {
            _notificationService.ShowWarning("Please describe what you were trying to do");
            WhatDoingTextBox.Focus();
            return;
        }

        var additionalNotes = AdditionalNotesTextBox.Text?.Trim();

        // Show loading state
        SubmitButton.IsEnabled = false;
        SubmitButton.Content = "Submitting...";
        CancelButton.IsEnabled = false;

        try
        {
            // Submit bug report
            var success = await _bugReportService.SubmitBugReportAsync(whatDoing, additionalNotes);

            if (success)
            {
                _notificationService.ShowSuccess("Bug report submitted! Thank you for your feedback.");
                _logger?.LogInformation("Bug report submitted successfully");
                Close();
            }
            else
            {
                _notificationService.ShowError("Failed to submit bug report. Please try again or contact support directly.");
                _logger?.LogWarning("Bug report submission failed");

                // Re-enable buttons
                SubmitButton.IsEnabled = true;
                SubmitButton.Content = "Submit Bug Report";
                CancelButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception while submitting bug report");
            _notificationService.ShowError($"Error submitting bug report: {ex.Message}");

            // Re-enable buttons
            SubmitButton.IsEnabled = true;
            SubmitButton.Content = "Submit Bug Report";
            CancelButton.IsEnabled = true;
        }
    }
}
