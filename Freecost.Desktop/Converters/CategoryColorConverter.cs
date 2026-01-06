using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Freecost.Desktop.Converters;

/// <summary>
/// Converts ingredient category names to color brushes for visual differentiation.
/// Uses predefined colors for common categories and generates consistent colors
/// based on hash for custom categories.
/// </summary>
public class CategoryColorConverter : IValueConverter
{
    // Predefined color palette for custom categories (vibrant, distinguishable colors)
    private static readonly string[] ColorPalette = new[]
    {
        "#E91E63", // Pink
        "#9C27B0", // Purple
        "#673AB7", // Deep Purple
        "#3F51B5", // Indigo
        "#2196F3", // Blue
        "#00BCD4", // Cyan
        "#009688", // Teal
        "#4CAF50", // Green
        "#8BC34A", // Light Green
        "#CDDC39", // Lime
        "#FFC107", // Amber
        "#FF9800", // Orange
        "#FF5722", // Deep Orange
        "#795548", // Brown
        "#607D8B", // Blue Grey
        "#F06292", // Light Pink
        "#BA68C8", // Light Purple
        "#9575CD", // Medium Purple
        "#7986CB", // Light Indigo
        "#64B5F6"  // Light Blue
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // This converter now returns a default color
        // The actual color assignment is done by CategoryColorService when the category is set
        if (value is not string category || string.IsNullOrWhiteSpace(category))
            return new SolidColorBrush(Color.Parse("#999999")); // Gray for no category

        // Return a default color - the actual color comes from the entity's CategoryColor property
        return new SolidColorBrush(Color.Parse("#999999"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generates a consistent color for a category name based on its hash.
    /// Same category name will always get the same color.
    /// </summary>
    private static string GetConsistentColorForCategory(string category)
    {
        // Use string hash code to pick a color from the palette
        var hashCode = category.GetHashCode();
        var index = Math.Abs(hashCode % ColorPalette.Length);
        return ColorPalette[index];
    }
}
