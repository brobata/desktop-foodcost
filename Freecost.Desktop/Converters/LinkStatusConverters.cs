using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Freecost.Desktop.Converters;

/// <summary>
/// Converts IsConnected boolean to link icon (🔗 for connected, ⛓️‍💥 for broken)
/// </summary>
public class BoolToLinkIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected ? "🔗" : "⛓️‍💥";
        }
        return "🔗";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsConnected boolean to background color (transparent - icon color shows status)
/// </summary>
public class BoolToLinkColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Return transparent background - the icon itself shows the status
        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsConnected boolean to tooltip text
/// </summary>
public class BoolToLinkTooltipConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected
                ? "Connected to ingredient/recipe. Click to change link."
                : "⚠️ Not linked - placeholder from import. Click to link to an ingredient or recipe.";
        }
        return "Click to manage link";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
