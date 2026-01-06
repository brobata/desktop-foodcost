using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Dfc.Desktop.Converters;

public class BoolToActiveButtonConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "Deactivate" : "Activate";
        }

        return "Toggle";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
