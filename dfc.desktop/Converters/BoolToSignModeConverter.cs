using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Dfc.Desktop.Converters;

public class BoolToSignModeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSignUpMode)
        {
            return isSignUpMode ? "Sign In Instead" : "Create Account";
        }
        return "Sign In";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
