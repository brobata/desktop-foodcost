// Location: Freecost.Core/Models/IngredientAlias.cs
// Action: CREATE this new file

using System;

namespace Freecost.Core.Models;

/// <summary>
/// Represents an alternative name for an ingredient
/// Enables flexible searching: "onion", "yellow onion", "onions" all match same ingredient
/// </summary>
public class IngredientAlias
{
    public Guid Id { get; set; }

    /// <summary>
    /// The alternative name for the ingredient
    /// </summary>
    public string AliasName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the primary/preferred alias shown in lists
    /// Only one alias per ingredient should be primary
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Foreign key to the ingredient
    /// </summary>
    public Guid IngredientId { get; set; }

    /// <summary>
    /// Navigation property to parent ingredient
    /// </summary>
    public Ingredient Ingredient { get; set; } = null!;

    /// <summary>
    /// When this alias was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}