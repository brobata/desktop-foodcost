using System;

namespace Freecost.Core.Models;

/// <summary>
/// Represents a photo attached to a recipe or entree
/// </summary>
public class Photo : BaseEntity
{
    public string FilePath { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int Order { get; set; } // Display order
    public bool IsPrimary { get; set; } // Primary/featured photo

    // Polymorphic relationship - photo can belong to Recipe or Entree
    public Guid? RecipeId { get; set; }
    public Guid? EntreeId { get; set; }

    // Navigation properties
    public Recipe? Recipe { get; set; }
    public Entree? Entree { get; set; }
}
