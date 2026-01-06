// Location: Freecost.Core/Models/EntreeRecipe.cs
// Action: REPLACE entire file

using Freecost.Core.Enums;
using System;

namespace Freecost.Core.Models;

public class EntreeRecipe : BaseEntity
{
    public Guid EntreeId { get; set; }
    public Guid RecipeId { get; set; }
    public decimal Quantity { get; set; }
    public UnitType Unit { get; set; } = UnitType.Each;
    public int SortOrder { get; set; }

    /// <summary>
    /// Display name for this recipe component (e.g., "Alfredo Sauce")
    /// Preserves the original name from entree card import, which may be simpler than the full database recipe name
    /// </summary>
    public string? DisplayName { get; set; }

    // Navigation properties
    public Entree Entree { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;
}