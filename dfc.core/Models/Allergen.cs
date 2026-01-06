using Dfc.Core.Enums;
using System.Collections.Generic;

namespace Dfc.Core.Models;

public class Allergen : BaseEntity
{
    public AllergenType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconPath { get; set; }

    // Navigation properties
    public ICollection<RecipeAllergen> RecipeAllergens { get; set; } = new List<RecipeAllergen>();
    public ICollection<EntreeAllergen> EntreeAllergens { get; set; } = new List<EntreeAllergen>();
}