using Freecost.Core.Enums;
using Freecost.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class DietComplianceService : IDietComplianceService
{
    public async Task<DietComplianceReport> CheckRecipeComplianceAsync(Recipe recipe, List<AllergenType> dietaryRequirements)
    {
        var report = new DietComplianceReport();

        if (recipe.RecipeAllergens == null || !recipe.RecipeAllergens.Any())
        {
            report.Recommendations.Add("No allergen information available. Please add allergen tags for accurate compliance checking.");
            return report;
        }

        var recipeAllergens = recipe.RecipeAllergens
            .Where(ra => ra.IsEnabled)
            .Select(ra => ra.Allergen.Type)
            .ToList();

        foreach (var requirement in dietaryRequirements)
        {
            var compliance = CheckDietaryCompliance(requirement, recipeAllergens, recipe);

            if (compliance.IsViolation)
            {
                report.Violations.Add(new DietViolation
                {
                    DietaryRequirement = requirement,
                    Reason = compliance.Reason,
                    ConflictingIngredients = compliance.ConflictingIngredients,
                    Suggestion = compliance.Suggestion
                });
            }
            else
            {
                report.MeetsDietaryRequirements.Add(requirement);
            }
        }

        report.IsCompliant = !report.Violations.Any();

        // Add general recommendations
        if (report.IsCompliant && report.MeetsDietaryRequirements.Any())
        {
            report.Recommendations.Add($"✅ This recipe complies with: {string.Join(", ", report.MeetsDietaryRequirements.Select(FormatAllergenType))}");
        }

        return await Task.FromResult(report);
    }

    public async Task<DietComplianceReport> CheckEntreeComplianceAsync(Entree entree, List<AllergenType> dietaryRequirements)
    {
        var report = new DietComplianceReport();

        if (entree.EntreeAllergens == null || !entree.EntreeAllergens.Any())
        {
            report.Recommendations.Add("No allergen information available. Please add allergen tags for accurate compliance checking.");
            return report;
        }

        var entreeAllergens = entree.EntreeAllergens
            .Where(ea => ea.IsEnabled)
            .Select(ea => ea.Allergen.Type)
            .ToList();

        foreach (var requirement in dietaryRequirements)
        {
            var compliance = CheckDietaryComplianceForEntree(requirement, entreeAllergens);

            if (compliance.IsViolation)
            {
                report.Violations.Add(new DietViolation
                {
                    DietaryRequirement = requirement,
                    Reason = compliance.Reason,
                    ConflictingIngredients = compliance.ConflictingIngredients,
                    Suggestion = compliance.Suggestion
                });
            }
            else
            {
                report.MeetsDietaryRequirements.Add(requirement);
            }
        }

        report.IsCompliant = !report.Violations.Any();

        if (report.IsCompliant && report.MeetsDietaryRequirements.Any())
        {
            report.Recommendations.Add($"✅ This entree complies with: {string.Join(", ", report.MeetsDietaryRequirements.Select(FormatAllergenType))}");
        }

        return await Task.FromResult(report);
    }

    public async Task<List<AllergenType>> GetRecommendedDietaryLabelsAsync(Recipe recipe)
    {
        var recommendedLabels = new List<AllergenType>();

        if (recipe.RecipeAllergens == null || !recipe.RecipeAllergens.Any())
        {
            return recommendedLabels;
        }

        var recipeAllergens = recipe.RecipeAllergens
            .Where(ra => ra.IsEnabled)
            .Select(ra => ra.Allergen.Type)
            .ToList();

        // Check for vegan
        if (!recipeAllergens.Contains(AllergenType.Milk) &&
            !recipeAllergens.Contains(AllergenType.Eggs) &&
            !recipeAllergens.Contains(AllergenType.Fish) &&
            !recipeAllergens.Contains(AllergenType.Shellfish))
        {
            recommendedLabels.Add(AllergenType.Vegan);
        }

        // Check for vegetarian
        if (!recipeAllergens.Contains(AllergenType.Fish) &&
            !recipeAllergens.Contains(AllergenType.Shellfish))
        {
            recommendedLabels.Add(AllergenType.Vegetarian);
        }

        // Check for pescatarian (allows fish but no other meat)
        if (!recipeAllergens.Contains(AllergenType.Fish) &&
            !recipeAllergens.Contains(AllergenType.Shellfish))
        {
            recommendedLabels.Add(AllergenType.Pescatarian);
        }

        // Check for gluten-free
        if (!recipeAllergens.Contains(AllergenType.Wheat))
        {
            recommendedLabels.Add(AllergenType.GlutenFree);
        }

        return await Task.FromResult(recommendedLabels);
    }

    public bool IsCompliantWith(Recipe recipe, AllergenType dietType)
    {
        if (recipe.RecipeAllergens == null || !recipe.RecipeAllergens.Any())
        {
            return false;
        }

        var recipeAllergens = recipe.RecipeAllergens
            .Where(ra => ra.IsEnabled)
            .Select(ra => ra.Allergen.Type)
            .ToList();

        var compliance = CheckDietaryCompliance(dietType, recipeAllergens, recipe);
        return !compliance.IsViolation;
    }

    private ComplianceCheck CheckDietaryCompliance(AllergenType requirement, List<AllergenType> allergens, Recipe recipe)
    {
        return requirement switch
        {
            AllergenType.Vegan => CheckVeganCompliance(allergens, recipe),
            AllergenType.Vegetarian => CheckVegetarianCompliance(allergens, recipe),
            AllergenType.Pescatarian => CheckPescatarianCompliance(allergens, recipe),
            AllergenType.GlutenFree => CheckGlutenFreeCompliance(allergens, recipe),
            AllergenType.Kosher => new ComplianceCheck { IsViolation = false, Reason = "Kosher compliance check requires manual verification" },
            AllergenType.Halal => new ComplianceCheck { IsViolation = false, Reason = "Halal compliance check requires manual verification" },
            _ => new ComplianceCheck { IsViolation = false }
        };
    }

    private ComplianceCheck CheckDietaryComplianceForEntree(AllergenType requirement, List<AllergenType> allergens)
    {
        return requirement switch
        {
            AllergenType.Vegan => CheckVeganComplianceSimple(allergens),
            AllergenType.Vegetarian => CheckVegetarianComplianceSimple(allergens),
            AllergenType.Pescatarian => CheckPescatarianComplianceSimple(allergens),
            AllergenType.GlutenFree => CheckGlutenFreeComplianceSimple(allergens),
            _ => new ComplianceCheck { IsViolation = false }
        };
    }

    private ComplianceCheck CheckVeganCompliance(List<AllergenType> allergens, Recipe recipe)
    {
        var animalProducts = new List<AllergenType> { AllergenType.Milk, AllergenType.Eggs, AllergenType.Fish, AllergenType.Shellfish };
        var violations = allergens.Intersect(animalProducts).ToList();

        if (violations.Any())
        {
            var conflictingIngredients = recipe.RecipeIngredients
                ?.Where(ri => ri.Ingredient?.IngredientAllergens?.Any(ia => violations.Contains(ia.Allergen.Type)) == true)
                .Select(ri => ri.Ingredient!.Name)
                .ToList() ?? new List<string>();

            return new ComplianceCheck
            {
                IsViolation = true,
                Reason = $"Contains animal products: {string.Join(", ", violations.Select(FormatAllergenType))}",
                ConflictingIngredients = conflictingIngredients,
                Suggestion = "Replace animal products with plant-based alternatives"
            };
        }

        return new ComplianceCheck { IsViolation = false };
    }

    private ComplianceCheck CheckVegetarianCompliance(List<AllergenType> allergens, Recipe recipe)
    {
        var meatProducts = new List<AllergenType> { AllergenType.Fish, AllergenType.Shellfish };
        var violations = allergens.Intersect(meatProducts).ToList();

        if (violations.Any())
        {
            return new ComplianceCheck
            {
                IsViolation = true,
                Reason = $"Contains meat products: {string.Join(", ", violations.Select(FormatAllergenType))}",
                Suggestion = "Remove meat products for vegetarian compliance"
            };
        }

        return new ComplianceCheck { IsViolation = false };
    }

    private ComplianceCheck CheckPescatarianCompliance(List<AllergenType> allergens, Recipe recipe)
    {
        // Pescatarian allows fish and shellfish, just not other meats
        // Since we don't track "meat" as an allergen type, we'll just check if it's marked as Pescatarian
        return new ComplianceCheck { IsViolation = false };
    }

    private ComplianceCheck CheckGlutenFreeCompliance(List<AllergenType> allergens, Recipe recipe)
    {
        if (allergens.Contains(AllergenType.Wheat))
        {
            var conflictingIngredients = recipe.RecipeIngredients
                ?.Where(ri => ri.Ingredient?.IngredientAllergens?.Any(ia => ia.Allergen.Type == AllergenType.Wheat) == true)
                .Select(ri => ri.Ingredient!.Name)
                .ToList() ?? new List<string>();

            return new ComplianceCheck
            {
                IsViolation = true,
                Reason = "Contains wheat/gluten",
                ConflictingIngredients = conflictingIngredients,
                Suggestion = "Use gluten-free alternatives (rice flour, almond flour, etc.)"
            };
        }

        return new ComplianceCheck { IsViolation = false };
    }

    private ComplianceCheck CheckVeganComplianceSimple(List<AllergenType> allergens)
    {
        var animalProducts = new List<AllergenType> { AllergenType.Milk, AllergenType.Eggs, AllergenType.Fish, AllergenType.Shellfish };
        var violations = allergens.Intersect(animalProducts).ToList();

        if (violations.Any())
        {
            return new ComplianceCheck
            {
                IsViolation = true,
                Reason = $"Contains animal products: {string.Join(", ", violations.Select(FormatAllergenType))}"
            };
        }

        return new ComplianceCheck { IsViolation = false };
    }

    private ComplianceCheck CheckVegetarianComplianceSimple(List<AllergenType> allergens)
    {
        var meatProducts = new List<AllergenType> { AllergenType.Fish, AllergenType.Shellfish };
        var violations = allergens.Intersect(meatProducts).ToList();

        if (violations.Any())
        {
            return new ComplianceCheck
            {
                IsViolation = true,
                Reason = $"Contains meat products: {string.Join(", ", violations.Select(FormatAllergenType))}"
            };
        }

        return new ComplianceCheck { IsViolation = false };
    }

    private ComplianceCheck CheckPescatarianComplianceSimple(List<AllergenType> allergens)
    {
        return new ComplianceCheck { IsViolation = false };
    }

    private ComplianceCheck CheckGlutenFreeComplianceSimple(List<AllergenType> allergens)
    {
        if (allergens.Contains(AllergenType.Wheat))
        {
            return new ComplianceCheck
            {
                IsViolation = true,
                Reason = "Contains wheat/gluten"
            };
        }

        return new ComplianceCheck { IsViolation = false };
    }

    private string FormatAllergenType(AllergenType allergen)
    {
        return allergen switch
        {
            AllergenType.TreeNuts => "Tree Nuts",
            AllergenType.GlutenFree => "Gluten-Free",
            AllergenType.ContainsAlcohol => "Contains Alcohol",
            _ => allergen.ToString()
        };
    }

    private class ComplianceCheck
    {
        public bool IsViolation { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<string> ConflictingIngredients { get; set; } = new();
        public string? Suggestion { get; set; }
    }
}
