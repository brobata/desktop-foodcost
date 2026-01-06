using System.Collections.Generic;

namespace Dfc.Desktop.Models;

/// <summary>
/// Represents a single step in a tutorial with content and annotations
/// </summary>
public class TutorialStep
{
    /// <summary>
    /// Unique identifier for this step
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of this tutorial step
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description explaining the feature
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Path to the screenshot image (relative to Assets/Tutorial/Screenshots/)
    /// </summary>
    public string? ScreenshotPath { get; set; }

    /// <summary>
    /// List of visual annotations to overlay on the screenshot
    /// </summary>
    public List<TutorialAnnotation> Annotations { get; set; } = new();

    /// <summary>
    /// Optional navigation target (e.g., "Ingredients" to navigate to that view)
    /// </summary>
    public string? NavigationTarget { get; set; }

    /// <summary>
    /// Optional keyboard shortcut to highlight
    /// </summary>
    public string? KeyboardShortcut { get; set; }

    /// <summary>
    /// Tips or pro advice for this feature
    /// </summary>
    public string? ProTip { get; set; }

    /// <summary>
    /// Order within the parent module
    /// </summary>
    public int StepNumber { get; set; }
}
