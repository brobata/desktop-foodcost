using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Dfc.Desktop.Converters;

public class StepIndicatorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int currentStep && parameter is string paramString && int.TryParse(paramString, out int targetStep))
        {
            bool isActive = currentStep == targetStep;

            // Check if we're converting to a Brush
            if (targetType == typeof(IBrush) || targetType == typeof(Brush))
            {
                // For Background
                return isActive
                    ? new SolidColorBrush(Color.Parse("#E8F5E9"))  // Green background when active
                    : new SolidColorBrush(Color.Parse("#F5F5F5")); // Gray background when inactive
            }

            // Otherwise return boolean for IsVisible bindings
            return isActive;
        }

        return new SolidColorBrush(Color.Parse("#F5F5F5")); // Default gray
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StepBorderConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int currentStep && parameter is string paramString && int.TryParse(paramString, out int targetStep))
        {
            bool isActive = currentStep == targetStep;

            // For BorderBrush
            return isActive
                ? new SolidColorBrush(Color.Parse("#7AB51D"))  // Green border when active
                : new SolidColorBrush(Color.Parse("#D0D0D0")); // Gray border when inactive
        }

        return new SolidColorBrush(Color.Parse("#D0D0D0")); // Default gray
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
