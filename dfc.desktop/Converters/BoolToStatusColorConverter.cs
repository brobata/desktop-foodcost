using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Dfc.Desktop.Converters;

public class BoolToStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            // Green for active, amber for inactive
            return new SolidColorBrush(Color.Parse(isActive ? "#E8F5E9" : "#FFF3E0"));
        }

        return new SolidColorBrush(Colors.LightGray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
