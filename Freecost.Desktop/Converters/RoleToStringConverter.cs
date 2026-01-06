using Avalonia.Data.Converters;
using Freecost.Core.Enums;
using System;
using System.Globalization;

namespace Freecost.Desktop.Converters;

public class RoleToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "All Roles";

        if (value is UserRole role)
        {
            return role.ToString();
        }

        return value.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            if (str == "All Roles")
                return null;

            if (Enum.TryParse<UserRole>(str, out var role))
                return role;
        }

        return null;
    }
}
