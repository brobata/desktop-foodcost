using Freecost.Core.Models;
using Freecost.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.Models;

public class RecipeDisplayModel
{
    private readonly Recipe _recipe;
    private readonly IRecipeCostCalculator? _costCalculator;

    public RecipeDisplayModel(Recipe recipe, IRecipeCostCalculator costCalculator)
    {
        _recipe = recipe;
        _costCalculator = costCalculator;
        _ = UpdateCostsAsync();
    }

    public RecipeDisplayModel(Recipe recipe)
    {
        _recipe = recipe;
        _costCalculator = null;
    }

    private async Task UpdateCostsAsync()
    {
        if (_costCalculator != null)
        {
            await _costCalculator.CalculateRecipeTotalCostAsync(_recipe);
        }
    }

    public Recipe Recipe => _recipe;

    // Display Properties
    public string Name => _recipe.Name;
    public string Description => _recipe.Description ?? string.Empty;
    public string YieldDisplay => $"{_recipe.Yield:F1} {_recipe.YieldUnit}";
    public string PrepTimeDisplay => _recipe.PrepTimeMinutes.HasValue ? $"{_recipe.PrepTimeMinutes}m" : "N/A";
    public string CostDisplay => _recipe.TotalCost.ToString("C2");
    public string Category => _recipe.Category ?? string.Empty;

    public string DifficultyDisplay => _recipe.Difficulty switch
    {
        Core.Enums.DifficultyLevel.Easy => "Easy",
        Core.Enums.DifficultyLevel.Medium => "Medium",
        Core.Enums.DifficultyLevel.Hard => "Hard",
        Core.Enums.DifficultyLevel.Expert => "Expert",
        _ => "-"
    };

    // Difficulty Badge Properties
    public string DifficultyBadge => _recipe.Difficulty switch
    {
        Core.Enums.DifficultyLevel.Easy => "⭐ Easy",
        Core.Enums.DifficultyLevel.Medium => "⭐⭐ Medium",
        Core.Enums.DifficultyLevel.Hard => "⭐⭐⭐ Hard",
        Core.Enums.DifficultyLevel.Expert => "⭐⭐⭐⭐ Expert",
        _ => "-"
    };

    public string DifficultyBadgeColor => _recipe.Difficulty switch
    {
        Core.Enums.DifficultyLevel.Easy => "#4CAF50",
        Core.Enums.DifficultyLevel.Medium => "#FF9800",
        Core.Enums.DifficultyLevel.Hard => "#F44336",
        Core.Enums.DifficultyLevel.Expert => "#9C27B0",
        _ => "#9E9E9E"
    };

    public string DifficultyBadgeBackground => _recipe.Difficulty switch
    {
        Core.Enums.DifficultyLevel.Easy => "#E8F5E9",
        Core.Enums.DifficultyLevel.Medium => "#FFF3E0",
        Core.Enums.DifficultyLevel.Hard => "#FFEBEE",
        Core.Enums.DifficultyLevel.Expert => "#F3E5F5",
        _ => "#F5F5F5"
    };

    // NEW: Additional properties for DataGrid
    public int IngredientCount => _recipe.RecipeIngredients?.Count ?? 0;
    public string TotalCost => _recipe.TotalCost.ToString("C2");
    public string CostPerUnit => _recipe.Yield > 0 ? (_recipe.TotalCost / _recipe.Yield).ToString("C2") : "$0.00";

    // Status Properties
    public string StatusIcon
    {
        get
        {
            var issues = GetValidationIssues();
            return issues.Count == 0 ? "✅" : "⚠️";
        }
    }

    public string StatusTooltip
    {
        get
        {
            var issues = GetValidationIssues();
            if (issues.Count == 0)
                return "Recipe is properly costed";

            return "Issues:\n" + string.Join("\n", issues.Select(i => $"• {i}"));
        }
    }

    public string StatusColor => GetValidationIssues().Count == 0 ? "#4CAF50" : "#FF9800";
    // Row text color - normal text for all recipes regardless of validation status
    public string RowForeground => "#2D2D2D";


    private List<string> GetValidationIssues()
    {
        var issues = new List<string>();

        if (_recipe.RecipeIngredients == null || !_recipe.RecipeIngredients.Any())
        {
            issues.Add("Recipe has no ingredients");
            return issues;
        }

        var unpricedIngredients = _recipe.RecipeIngredients
            .Where(ri => ri.Ingredient != null && ri.Ingredient.CurrentPrice <= 0)
            .ToList();

        if (unpricedIngredients.Any())
        {
            issues.Add($"{unpricedIngredients.Count} ingredient(s) have no price");
        }

        var incompleteConversions = _recipe.RecipeIngredients
            .Where(ri => ri.Ingredient != null &&
                         ri.Ingredient.UseAlternateUnit &&
                         (!ri.Ingredient.AlternateConversionQuantity.HasValue ||
                          !ri.Ingredient.AlternateConversionUnit.HasValue))
            .ToList();

        if (incompleteConversions.Any())
        {
            issues.Add($"{incompleteConversions.Count} ingredient(s) have incomplete unit conversions");
        }

        return issues;
    }
}