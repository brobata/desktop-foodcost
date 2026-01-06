using System;
using System.ComponentModel.DataAnnotations;

namespace Dfc.Core.Models;

/// <summary>
/// Global allergen keyword mapping stored in Firebase
/// Maps ingredient keywords to allergen types for auto-detection
/// </summary>
public class GlobalAllergenKeyword
{
    /// <summary>
    /// Firestore document ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Keyword to search for in ingredient names (e.g., "milk", "cream", "butter")
    /// Normalized: lowercase, trimmed
    /// </summary>
    [Required]
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Allergen type this keyword indicates (e.g., "Milk", "Eggs", "TreeNuts")
    /// Stored as string, mapped to AllergenType enum
    /// </summary>
    [Required]
    public string AllergenType { get; set; } = string.Empty;

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
    /// Optional notes (e.g., "Common in dairy products")
    /// </summary>
    public string? Notes { get; set; }
}
