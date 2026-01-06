using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Freecost.Core.Services;
using System;
using System.Threading;

namespace Freecost.Desktop.Controls;

public partial class StatusNotificationBar : UserControl
{
    private TextBlock? _statusIcon;
    private TextBlock? _statusMessage;
    private Border? _stoplightBorder;
    private Button? _dismissButton;
    private TextBlock? _buildNumber;
    private Timer? _autoDismissTimer;

    public StatusNotificationBar()
    {
        InitializeComponent();
        InitializeControls();
    }

    private void InitializeControls()
    {
        _statusIcon = this.FindControl<TextBlock>("StatusIcon");
        _statusMessage = this.FindControl<TextBlock>("StatusMessage");
        _stoplightBorder = this.FindControl<Border>("StoplightBorder");
        _dismissButton = this.FindControl<Button>("DismissButton");
        _buildNumber = this.FindControl<TextBlock>("BuildNumber");

        if (_statusMessage != null)
        {
            _statusMessage.Text = "Ready";
        }

        // Set version number from assembly
        if (_buildNumber != null)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                // Show version in format: v0.10.0
                _buildNumber.Text = $"v{version.Major}.{version.Minor}.{version.Build}";
            }
        }
    }

    public void ShowNotification(StatusNotificationEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_statusIcon == null || _statusMessage == null || _dismissButton == null) return;

            // Set icon and color based on notification level
            switch (e.Level)
            {
                case NotificationLevel.Success:
                    _statusIcon.Text = "✓";
                    _statusIcon.Foreground = new SolidColorBrush(Color.Parse("#4CAF50")); // Green
                    _statusMessage.Foreground = new SolidColorBrush(Color.Parse("#2E7D32"));
                    break;

                case NotificationLevel.Info:
                    _statusIcon.Text = "ℹ";
                    _statusIcon.Foreground = new SolidColorBrush(Color.Parse("#2196F3")); // Blue
                    _statusMessage.Foreground = new SolidColorBrush(Color.Parse("#1565C0"));
                    break;

                case NotificationLevel.Warning:
                    _statusIcon.Text = "⚠";
                    _statusIcon.Foreground = new SolidColorBrush(Color.Parse("#FF9800")); // Orange
                    _statusMessage.Foreground = new SolidColorBrush(Color.Parse("#E65100"));
                    break;

                case NotificationLevel.Error:
                    _statusIcon.Text = "✗";
                    _statusIcon.Foreground = new SolidColorBrush(Color.Parse("#F44336")); // Red
                    _statusMessage.Foreground = new SolidColorBrush(Color.Parse("#C62828"));
                    break;
            }

            _statusIcon.IsVisible = true;
            _statusMessage.Text = e.Message;

            // Show dismiss button for errors (which don't auto-dismiss)
            _dismissButton.IsVisible = e.Level == NotificationLevel.Error;

            // Auto-dismiss if duration is set
            if (e.DurationMs > 0)
            {
                _autoDismissTimer?.Dispose();
                _autoDismissTimer = new Timer(_ =>
                {
                    Dispatcher.UIThread.Post(ClearNotification);
                }, null, e.DurationMs, Timeout.Infinite);
            }
        });
    }

    public void UpdateStoplight(string color, string tooltip)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_stoplightBorder == null) return;

            _stoplightBorder.Background = new SolidColorBrush(Color.Parse(color));
            ToolTip.SetTip(_stoplightBorder, tooltip);
        });
    }

    private void ClearNotification()
    {
        if (_statusIcon == null || _statusMessage == null || _dismissButton == null) return;

        _statusIcon.IsVisible = false;
        _statusMessage.Text = "Ready";
        _statusMessage.Foreground = new SolidColorBrush(Color.Parse("#666666"));
        _dismissButton.IsVisible = false;
    }

    private void OnDismissClicked(object? sender, RoutedEventArgs e)
    {
        ClearNotification();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _autoDismissTimer?.Dispose();
    }
}
