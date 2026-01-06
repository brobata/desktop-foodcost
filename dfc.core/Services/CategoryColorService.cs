using System;
using System.Collections.Generic;

namespace Dfc.Core.Services;

/// <summary>
/// Provides consistent color assignment for categories across the application.
/// Uses a hash-based algorithm to ensure the same category always gets the same color,
/// making it easy for users to visually identify categories across different views.
/// </summary>
public interface ICategoryColorService
{
    /// <summary>
    /// Get or assign a consistent color for a category.
    /// Returns the existing color if the category already has one, otherwise assigns a new color
    /// based on a hash of the category name to ensure consistency across the application.
    /// </summary>
    /// <param name="category">The category name to get a color for</param>
    /// <param name="existingColor">The existing color already assigned to this entity (if any)</param>
    /// <returns>A hex color code (e.g., "#E91E63") for the category</returns>
    /// <remarks>
    /// If existingColor is provided and not empty, it will be returned unchanged to preserve user's color choices.
    /// If no category is provided, returns gray (#999999).
    /// Colors are deterministic - the same category name always produces the same color.
    /// </remarks>
    string GetOrAssignColor(string? category, string? existingColor = null);
}

/// <summary>
/// Implementation of category color service using a predefined color palette
/// and hash-based assignment for consistency.
/// </summary>
public class CategoryColorService : ICategoryColorService
{
    // Predefined color palette for categories (vibrant, distinguishable colors)
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

    public string GetOrAssignColor(string? category, string? existingColor = null)
    {
        // If no category, return gray
        if (string.IsNullOrWhiteSpace(category))
            return "#999999";

        // If entity already has a color assigned, use it
        if (!string.IsNullOrWhiteSpace(existingColor))
            return existingColor;

        // Generate consistent color based on category name hash
        var hashCode = category.GetHashCode();
        var index = Math.Abs(hashCode % ColorPalette.Length);
        return ColorPalette[index];
    }
}
