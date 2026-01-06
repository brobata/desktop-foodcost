using Avalonia.Data.Converters;
using Dfc.Core.Enums;
using Dfc.Core.Helpers;
using System;
using System.Globalization;

namespace Dfc.Desktop.Converters;

public class UnitTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is UnitType unitType)
        {
            return UnitConverter.GetAbbreviation(unitType);
        }
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}