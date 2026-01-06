using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Dfc.Desktop.Converters;

public class BoolToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string stringParam)
        {
            // Parameter format: "trueString|falseString"
            var strings = stringParam.Split('|');
            if (strings.Length == 2)
            {
                return boolValue ? strings[0] : strings[1];
            }
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
