namespace Freecost.Desktop.Models;

/// <summary>
/// Represents a visual annotation (arrow, highlight, callout) on a tutorial screenshot
/// </summary>
public class TutorialAnnotation
{
    /// <summary>
    /// Type of annotation to display
    /// </summary>
    public TutorialAnnotationType Type { get; set; }

    /// <summary>
    /// X coordinate (percentage of image width, 0-100)
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate (percentage of image height, 0-100)
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Width (for highlights and callouts, percentage of image width)
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// Height (for highlights and callouts, percentage of image height)
    /// </summary>
    public double? Height { get; set; }

    /// <summary>
    /// Text content (for callouts and badges)
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Direction for arrows (in degrees, 0=right, 90=down, 180=left, 270=up)
    /// </summary>
    public double? Direction { get; set; }

    /// <summary>
    /// Length of arrow (percentage of image width)
    /// </summary>
    public double? Length { get; set; }

    /// <summary>
    /// Color override (hex color code like "#7AB51D")
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Whether this annotation should pulse/animate for emphasis
    /// </summary>
    public bool Animate { get; set; }
}

/// <summary>
/// Types of visual annotations available
/// </summary>
public enum TutorialAnnotationType
{
    /// <summary>
    /// Arrow pointing to a feature
    /// </summary>
    Arrow,

    /// <summary>
    /// Rectangular highlight box around a feature
    /// </summary>
    Highlight,

    /// <summary>
    /// Circular highlight around a feature
    /// </summary>
    CircleHighlight,

    /// <summary>
    /// Text callout bubble with pointer
    /// </summary>
    Callout,

    /// <summary>
    /// Numbered badge (1, 2, 3, etc.)
    /// </summary>
    NumberBadge,

    /// <summary>
    /// Simple text label
    /// </summary>
    Label
}
