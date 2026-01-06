using Avalonia;
using Avalonia.Controls;
using System;

namespace Freecost.Desktop.Services;

/// <summary>
/// Service for responsive design across different screen sizes
/// </summary>
public class ResponsiveDesignService
{
    public enum ScreenSize
    {
        ExtraSmall,  // < 768px (phones)
        Small,       // 768-1024px (tablets portrait)
        Medium,      // 1024-1366px (tablets landscape, small laptops)
        Large,       // 1366-1920px (laptops, desktops)
        ExtraLarge   // > 1920px (large desktops, 4K)
    }

    public enum Orientation
    {
        Portrait,
        Landscape
    }

    /// <summary>
    /// Get screen size category based on width
    /// </summary>
    public static ScreenSize GetScreenSize(double width)
    {
        return width switch
        {
            < 768 => ScreenSize.ExtraSmall,
            < 1024 => ScreenSize.Small,
            < 1366 => ScreenSize.Medium,
            < 1920 => ScreenSize.Large,
            _ => ScreenSize.ExtraLarge
        };
    }

    /// <summary>
    /// Get orientation based on dimensions
    /// </summary>
    public static Orientation GetOrientation(double width, double height)
    {
        return width > height ? Orientation.Landscape : Orientation.Portrait;
    }

    /// <summary>
    /// Calculate responsive column count for grids
    /// </summary>
    public static int GetResponsiveColumns(double containerWidth, double minItemWidth = 200)
    {
        var screenSize = GetScreenSize(containerWidth);

        return screenSize switch
        {
            ScreenSize.ExtraSmall => 1,
            ScreenSize.Small => 2,
            ScreenSize.Medium => 3,
            ScreenSize.Large => 4,
            _ => Math.Max(1, (int)(containerWidth / minItemWidth))
        };
    }

    /// <summary>
    /// Get responsive font size
    /// </summary>
    public static double GetResponsiveFontSize(ScreenSize screenSize, double baseFontSize = 14)
    {
        return screenSize switch
        {
            ScreenSize.ExtraSmall => baseFontSize * 0.9,
            ScreenSize.Small => baseFontSize * 0.95,
            ScreenSize.Medium => baseFontSize,
            ScreenSize.Large => baseFontSize * 1.05,
            ScreenSize.ExtraLarge => baseFontSize * 1.1,
            _ => baseFontSize
        };
    }

    /// <summary>
    /// Get responsive padding
    /// </summary>
    public static Thickness GetResponsivePadding(ScreenSize screenSize, double basePadding = 16)
    {
        var padding = screenSize switch
        {
            ScreenSize.ExtraSmall => basePadding * 0.5,
            ScreenSize.Small => basePadding * 0.75,
            ScreenSize.Medium => basePadding,
            ScreenSize.Large => basePadding * 1.25,
            ScreenSize.ExtraLarge => basePadding * 1.5,
            _ => basePadding
        };

        return new Thickness(padding);
    }

    /// <summary>
    /// Get responsive margin
    /// </summary>
    public static Thickness GetResponsiveMargin(ScreenSize screenSize, double baseMargin = 8)
    {
        var margin = screenSize switch
        {
            ScreenSize.ExtraSmall => baseMargin * 0.5,
            ScreenSize.Small => baseMargin * 0.75,
            ScreenSize.Medium => baseMargin,
            ScreenSize.Large => baseMargin * 1.25,
            ScreenSize.ExtraLarge => baseMargin * 1.5,
            _ => baseMargin
        };

        return new Thickness(margin);
    }

    /// <summary>
    /// Get responsive width for dialogs
    /// </summary>
    public static double GetResponsiveDialogWidth(double screenWidth, ScreenSize screenSize)
    {
        return screenSize switch
        {
            ScreenSize.ExtraSmall => screenWidth * 0.95,  // 95% of screen
            ScreenSize.Small => screenWidth * 0.85,       // 85% of screen
            ScreenSize.Medium => Math.Min(800, screenWidth * 0.75),
            ScreenSize.Large => Math.Min(1000, screenWidth * 0.65),
            ScreenSize.ExtraLarge => Math.Min(1200, screenWidth * 0.55),
            _ => 800
        };
    }

    /// <summary>
    /// Get responsive height for dialogs
    /// </summary>
    public static double GetResponsiveDialogHeight(double screenHeight, ScreenSize screenSize)
    {
        return screenSize switch
        {
            ScreenSize.ExtraSmall => screenHeight * 0.9,
            ScreenSize.Small => screenHeight * 0.85,
            ScreenSize.Medium => Math.Min(600, screenHeight * 0.75),
            ScreenSize.Large => Math.Min(800, screenHeight * 0.7),
            ScreenSize.ExtraLarge => Math.Min(900, screenHeight * 0.65),
            _ => 600
        };
    }

    /// <summary>
    /// Get responsive DataGrid row height
    /// </summary>
    public static double GetResponsiveRowHeight(ScreenSize screenSize)
    {
        return screenSize switch
        {
            ScreenSize.ExtraSmall => 44,  // Touch-friendly
            ScreenSize.Small => 40,
            ScreenSize.Medium => 36,
            ScreenSize.Large => 32,
            ScreenSize.ExtraLarge => 32,
            _ => 36
        };
    }

    /// <summary>
    /// Get responsive button size
    /// </summary>
    public static Size GetResponsiveButtonSize(ScreenSize screenSize, bool isTouch = false)
    {
        var baseWidth = isTouch ? 120 : 100;
        var baseHeight = isTouch ? 44 : 32;

        var scale = screenSize switch
        {
            ScreenSize.ExtraSmall => 1.1,  // Larger for touch
            ScreenSize.Small => 1.05,
            ScreenSize.Medium => 1.0,
            ScreenSize.Large => 0.95,
            ScreenSize.ExtraLarge => 0.9,
            _ => 1.0
        };

        return new Size(baseWidth * scale, baseHeight * scale);
    }

    /// <summary>
    /// Check if device is likely touch-enabled
    /// </summary>
    public static bool IsTouchDevice(ScreenSize screenSize)
    {
        return screenSize is ScreenSize.ExtraSmall or ScreenSize.Small;
    }

    /// <summary>
    /// Get responsive icon size
    /// </summary>
    public static double GetResponsiveIconSize(ScreenSize screenSize)
    {
        return screenSize switch
        {
            ScreenSize.ExtraSmall => 20,
            ScreenSize.Small => 18,
            ScreenSize.Medium => 16,
            ScreenSize.Large => 16,
            ScreenSize.ExtraLarge => 18,
            _ => 16
        };
    }

    /// <summary>
    /// Set up responsive layout for a window
    /// </summary>
    public static void SetupResponsiveWindow(Window window, Action<ScreenSize, Orientation>? onSizeChanged = null)
    {
        void UpdateLayout(object? sender, EventArgs e)
        {
            var bounds = window.Bounds;
            var screenSize = GetScreenSize(bounds.Width);
            var orientation = GetOrientation(bounds.Width, bounds.Height);

            onSizeChanged?.Invoke(screenSize, orientation);
        }

        window.SizeChanged += UpdateLayout;

        // Initial update
        UpdateLayout(null, EventArgs.Empty);
    }

    /// <summary>
    /// Get recommended window size for screen
    /// </summary>
    public static Size GetRecommendedWindowSize(Screens screens)
    {
        var primaryScreen = screens.Primary;
        if (primaryScreen == null)
            return new Size(1200, 800);

        var workingArea = primaryScreen.WorkingArea;
        var screenSize = GetScreenSize(workingArea.Width);

        return screenSize switch
        {
            ScreenSize.ExtraSmall => new Size(workingArea.Width * 0.95, workingArea.Height * 0.95),
            ScreenSize.Small => new Size(workingArea.Width * 0.9, workingArea.Height * 0.9),
            ScreenSize.Medium => new Size(Math.Min(1024, workingArea.Width * 0.85), Math.Min(768, workingArea.Height * 0.85)),
            ScreenSize.Large => new Size(Math.Min(1366, workingArea.Width * 0.8), Math.Min(900, workingArea.Height * 0.8)),
            ScreenSize.ExtraLarge => new Size(Math.Min(1600, workingArea.Width * 0.75), Math.Min(1000, workingArea.Height * 0.75)),
            _ => new Size(1200, 800)
        };
    }

    /// <summary>
    /// Apply responsive styles to control
    /// </summary>
    public static void ApplyResponsiveStyles(Control control, ScreenSize screenSize)
    {
        // Set font size
        if (control is TextBlock textBlock)
        {
            var currentFontSize = textBlock.FontSize;
            textBlock.FontSize = GetResponsiveFontSize(screenSize, currentFontSize);
        }

        // Set padding for containers
        if (control is Border border)
        {
            border.Padding = GetResponsivePadding(screenSize);
        }

        // Set DataGrid row height
        if (control is DataGrid dataGrid)
        {
            dataGrid.RowHeight = GetResponsiveRowHeight(screenSize);
        }
    }
}

/// <summary>
/// Extension methods for responsive design
/// </summary>
public static class ResponsiveExtensions
{
    /// <summary>
    /// Make a window responsive
    /// </summary>
    public static void MakeResponsive(this Window window, Action<ResponsiveDesignService.ScreenSize, ResponsiveDesignService.Orientation>? onSizeChanged = null)
    {
        ResponsiveDesignService.SetupResponsiveWindow(window, onSizeChanged);
    }

    /// <summary>
    /// Apply responsive styles to a control
    /// </summary>
    public static void ApplyResponsiveStyles(this Control control, ResponsiveDesignService.ScreenSize screenSize)
    {
        ResponsiveDesignService.ApplyResponsiveStyles(control, screenSize);
    }
}
