using Dfc.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfc.Core.Models;

/// <summary>
/// Represents a recipe component within another recipe (recipe-in-recipe relationship)
/// Example: "Burger Sauce" recipe contains "Mayo Base" recipe as a sub-component
/// </summary>
public class RecipeRecipe
{
    public Guid Id { get; set; }

    /// <summary>
    /// The parent recipe that contains this component
    /// </summary>
    public Guid ParentRecipeId { get; set; }

    /// <summary>
    /// The recipe being used as a component/sub-recipe
    /// </summary>
    public Guid ComponentRecipeId { get; set; }

    public decimal Quantity { get; set; }

    public UnitType Unit { get; set; }

    /// <summary>
    /// Optional override name for display (used in imports)
    /// If set, this name is shown instead of the actual recipe name
    /// </summary>
    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public int SortOrder { get; set; }

    // Navigation properties
    public Recipe ParentRecipe { get; set; } = null!;
    public Recipe ComponentRecipe { get; set; } = null!;
}
