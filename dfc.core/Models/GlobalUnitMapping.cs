using System;
using System.ComponentModel.DataAnnotations;

namespace Dfc.Core.Models;

/// <summary>
/// Global unit conversion mapping stored in Firebase
/// Maps unit name variations to standardized UnitType enum values
/// </summary>
public class GlobalUnitMapping
{
    /// <summary>
    /// Firestore document ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Unit text as it appears in imports (e.g., "lb", "lbs", "pound", "pounds", "#")
    /// Normalized: lowercase, no spaces
    /// </summary>
    [Required]
    public string UnitText { get; set; } = string.Empty;

    /// <summary>
    /// Target UnitType enum value as string (e.g., "Pound", "Ounce", "Gallon")
    /// </summary>
    [Required]
    public string TargetUnit { get; set; } = string.Empty;

    /// <summary>
    /// When this mapping was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this mapping was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Who created/manages this mapping
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }
}
