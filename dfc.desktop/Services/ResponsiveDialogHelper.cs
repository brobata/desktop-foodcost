using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Dfc.Desktop.Services;

/// <summary>
/// Helper for creating mobile-friendly, responsive dialogs
/// </summary>
public static class ResponsiveDialogHelper
{
    /// <summary>
    /// Create a responsive dialog window
    /// </summary>
    public static Window CreateResponsiveDialog(string title, Control content, Window? owner = null)
    {
        var window = new Window
        {
            Title = title,
            Content = content,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = true,
            Background = Brushes.White
        };

        // Set responsive size based on screen
        SetResponsiveSize(window, owner);

        // Make window responsive to size changes
        window.MakeResponsive((screenSize, orientation) =>
        {
            // Adjust layout based on screen size
            ApplyResponsiveLayout(window, screenSize, orientation);
        });

        return window;
    }

    /// <summary>
    /// Set responsive size for a dialog
    /// </summary>
    public static void SetResponsiveSize(Window dialog, Window? owner = null)
    {
        var screens = dialog.Screens;
        var recommendedSize = ResponsiveDesignService.GetRecommendedWindowSize(screens);

        // Get screen bounds
        var primaryScreen = screens.Primary;
        if (primaryScreen != null)
        {
            var workingArea = primaryScreen.WorkingArea;
            var screenSize = ResponsiveDesignService.GetScreenSize(workingArea.Width);

            // Set dialog size based on screen size
            var dialogWidth = ResponsiveDesignService.GetResponsiveDialogWidth(workingArea.Width, screenSize);
            var dialogHeight = ResponsiveDesignService.GetResponsiveDialogHeight(workingArea.Height, screenSize);

            dialog.Width = dialogWidth;
            dialog.Height = dialogHeight;

            // Set minimum size for touch devices
            if (ResponsiveDesignService.IsTouchDevice(screenSize))
            {
                dialog.MinWidth = Math.Min(300, workingArea.Width * 0.9);
                dialog.MinHeight = Math.Min(400, workingArea.Height * 0.9);
            }
            else
            {
                dialog.MinWidth = 400;
                dialog.MinHeight = 300;
            }

            // Set maximum size
            dialog.MaxWidth = workingArea.Width * 0.95;
            dialog.MaxHeight = workingArea.Height * 0.95;
        }
    }

    /// <summary>
    /// Apply responsive layout to dialog content
    /// </summary>
    private static void ApplyResponsiveLayout(Window dialog, ResponsiveDesignService.ScreenSize screenSize, ResponsiveDesignService.Orientation orientation)
    {
        // Adjust padding and margins based on screen size
        if (dialog.Content is Control contentControl)
        {
            contentControl.ApplyResponsiveStyles(screenSize);
        }

        // Update font sizes
        UpdateFontSizes(dialog, screenSize);

        // Update button sizes for touch
        UpdateButtonSizes(dialog, screenSize);
    }

    /// <summary>
    /// Update font sizes throughout the dialog
    /// </summary>
    private static void UpdateFontSizes(Control control, ResponsiveDesignService.ScreenSize screenSize)
    {
        if (control is TextBlock textBlock && textBlock.FontSize > 0)
        {
            var baseFontSize = textBlock.FontSize;
            textBlock.FontSize = ResponsiveDesignService.GetResponsiveFontSize(screenSize, baseFontSize);
        }

        // Recursively update child controls
        if (control is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is Control childControl)
                {
                    UpdateFontSizes(childControl, screenSize);
                }
            }
        }
        else if (control is ContentControl contentControl && contentControl.Content is Control childContent)
        {
            UpdateFontSizes(childContent, screenSize);
        }
    }

    /// <summary>
    /// Update button sizes for touch-friendly interaction
    /// </summary>
    private static void UpdateButtonSizes(Control control, ResponsiveDesignService.ScreenSize screenSize)
    {
        var isTouch = ResponsiveDesignService.IsTouchDevice(screenSize);

        if (control is Button button)
        {
            var buttonSize = ResponsiveDesignService.GetResponsiveButtonSize(screenSize, isTouch);

            if (button.MinHeight < buttonSize.Height)
            {
                button.MinHeight = buttonSize.Height;
            }

            if (isTouch && button.Padding == default)
            {
                button.Padding = new Thickness(16, 12);
            }
        }

        // Recursively update child controls
        if (control is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is Control childControl)
                {
                    UpdateButtonSizes(childControl, screenSize);
                }
            }
        }
        else if (control is ContentControl contentControl && contentControl.Content is Control childContent)
        {
            UpdateButtonSizes(childContent, screenSize);
        }
    }

    /// <summary>
    /// Create a mobile-friendly confirmation dialog
    /// </summary>
    public static async System.Threading.Tasks.Task<bool> ShowConfirmationAsync(
        Window owner,
        string title,
        string message,
        string confirmText = "OK",
        string cancelText = "Cancel")
    {
        var window = new Window
        {
            Title = title,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = Brushes.White
        };

        SetResponsiveSize(window, owner);

        var panel = new StackPanel
        {
            Spacing = 20,
            Margin = new Thickness(24)
        };

        // Message
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#333"))
        });

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var cancelButton = new Button
        {
            Content = cancelText,
            MinWidth = 100,
            MinHeight = 36,
            Padding = new Thickness(16, 8)
        };
        cancelButton.Click += (s, e) => window.Close(false);

        var confirmButton = new Button
        {
            Content = confirmText,
            MinWidth = 100,
            MinHeight = 36,
            Padding = new Thickness(16, 8),
            Background = new SolidColorBrush(Color.Parse("#7AB51D")),
            Foreground = Brushes.White
        };
        confirmButton.Click += (s, e) => window.Close(true);

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(confirmButton);
        panel.Children.Add(buttonPanel);

        window.Content = panel;
        window.MakeResponsive();

        return await window.ShowDialog<bool>(owner);
    }

    /// <summary>
    /// Create a mobile-friendly message dialog
    /// </summary>
    public static async System.Threading.Tasks.Task ShowMessageAsync(
        Window owner,
        string title,
        string message,
        string buttonText = "OK")
    {
        var window = new Window
        {
            Title = title,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = Brushes.White
        };

        SetResponsiveSize(window, owner);

        var panel = new StackPanel
        {
            Spacing = 20,
            Margin = new Thickness(24)
        };

        // Message
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#333"))
        });

        // Button
        var okButton = new Button
        {
            Content = buttonText,
            MinWidth = 100,
            MinHeight = 36,
            Padding = new Thickness(16, 8),
            HorizontalAlignment = HorizontalAlignment.Right,
            Background = new SolidColorBrush(Color.Parse("#7AB51D")),
            Foreground = Brushes.White
        };
        okButton.Click += (s, e) => window.Close();

        panel.Children.Add(okButton);

        window.Content = panel;
        window.MakeResponsive();

        await window.ShowDialog(owner);
    }

    /// <summary>
    /// Create a mobile-friendly input dialog
    /// </summary>
    public static async System.Threading.Tasks.Task<string?> ShowInputAsync(
        Window owner,
        string title,
        string prompt,
        string defaultValue = "",
        string okText = "OK",
        string cancelText = "Cancel")
    {
        var window = new Window
        {
            Title = title,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = Brushes.White
        };

        SetResponsiveSize(window, owner);

        var panel = new StackPanel
        {
            Spacing = 16,
            Margin = new Thickness(24)
        };

        // Prompt
        panel.Children.Add(new TextBlock
        {
            Text = prompt,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#333"))
        });

        // Input
        var textBox = new TextBox
        {
            Text = defaultValue,
            MinHeight = 36,
            FontSize = 14
        };
        panel.Children.Add(textBox);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 8, 0, 0)
        };

        var cancelButton = new Button
        {
            Content = cancelText,
            MinWidth = 100,
            MinHeight = 36,
            Padding = new Thickness(16, 8)
        };
        cancelButton.Click += (s, e) => window.Close(null);

        var okButton = new Button
        {
            Content = okText,
            MinWidth = 100,
            MinHeight = 36,
            Padding = new Thickness(16, 8),
            Background = new SolidColorBrush(Color.Parse("#7AB51D")),
            Foreground = Brushes.White
        };
        okButton.Click += (s, e) => window.Close(textBox.Text);

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(okButton);
        panel.Children.Add(buttonPanel);

        window.Content = panel;
        window.MakeResponsive();

        // Focus the text box
        textBox.AttachedToVisualTree += (s, e) => textBox.Focus();
        textBox.SelectAll();

        return await window.ShowDialog<string?>(owner);
    }

    /// <summary>
    /// Apply touch-friendly spacing to a container
    /// </summary>
    public static void ApplyTouchFriendlySpacing(Panel panel, ResponsiveDesignService.ScreenSize screenSize)
    {
        if (!ResponsiveDesignService.IsTouchDevice(screenSize)) return;

        // Increase spacing for touch devices
        if (panel is StackPanel stackPanel)
        {
            stackPanel.Spacing = Math.Max(stackPanel.Spacing, 16);
        }

        // Increase button padding
        foreach (var child in panel.Children)
        {
            if (child is Button button && button.Padding == default)
            {
                button.Padding = new Thickness(16, 12);
                button.MinHeight = 44; // Touch-friendly minimum
            }
        }
    }
}
