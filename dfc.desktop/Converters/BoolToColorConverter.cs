using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Dfc.Desktop.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string colorParam)
        {
            // Parameter format: "trueColor|falseColor"
            var colors = colorParam.Split('|');
            if (colors.Length == 2)
            {
                var color = boolValue ? colors[0] : colors[1];

                // Return as SolidColorBrush if target is brush type
                if (targetType == typeof(IBrush) || targetType == typeof(SolidColorBrush))
                {
                    return new SolidColorBrush(Color.Parse(color));
                }

                // Return as Color if target is color type
                return Color.Parse(color);
            }
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
