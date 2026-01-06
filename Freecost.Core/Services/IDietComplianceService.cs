using Freecost.Core.Enums;
using Freecost.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IDietComplianceService
{
    /// <summary>
    /// Check if a recipe complies with dietary requirements
    /// </summary>
    Task<DietComplianceReport> CheckRecipeComplianceAsync(Recipe recipe, List<AllergenType> dietaryRequirements);

    /// <summary>
    /// Check if an entree complies with dietary requirements
    /// </summary>
    Task<DietComplianceReport> CheckEntreeComplianceAsync(Entree entree, List<AllergenType> dietaryRequirements);

    /// <summary>
    /// Get recommended dietary labels for a recipe based on its ingredients
    /// </summary>
    Task<List<AllergenType>> GetRecommendedDietaryLabelsAsync(Recipe recipe);

    /// <summary>
    /// Check if a recipe is compliant with a specific diet
    /// </summary>
    bool IsCompliantWith(Recipe recipe, AllergenType dietType);
}

public class DietComplianceReport
{
    public bool IsCompliant { get; set; }
    public List<AllergenType> MeetsDietaryRequirements { get; set; } = new();
    public List<DietViolation> Violations { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class DietViolation
{
    public AllergenType DietaryRequirement { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> ConflictingIngredients { get; set; } = new();
    public string? Suggestion { get; set; }
}
