using System.Collections.Generic;

namespace Dfc.Desktop.Models;

/// <summary>
/// Represents a complete tutorial module with multiple steps
/// </summary>
public class TutorialModule
{
    /// <summary>
    /// Unique identifier for this tutorial module
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the tutorial module
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of what this module teaches
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Icon or emoji to display for this module
    /// </summary>
    public string Icon { get; set; } = "📚";

    /// <summary>
    /// Ordered list of tutorial steps in this module
    /// </summary>
    public List<TutorialStep> Steps { get; set; } = new();

    /// <summary>
    /// Estimated time to complete this module in minutes
    /// </summary>
    public int EstimatedMinutes { get; set; }

    /// <summary>
    /// Order in which this module should appear
    /// </summary>
    public int DisplayOrder { get; set; }
}
