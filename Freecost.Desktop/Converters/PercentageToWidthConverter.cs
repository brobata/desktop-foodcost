using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Freecost.Desktop.Converters;

public class PercentageToWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            // Convert percentage (0-100) to width (assuming max width of 300)
            return (percentage / 100.0) * 300.0;
        }

        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
