using CommunityToolkit.Mvvm.ComponentModel;
using Dfc.Core.Enums;
using Dfc.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dfc.Desktop.ViewModels;

public partial class RecipeComparisonViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<RecipeComparisonItem> _recipes = new();

    public bool HasNutrition => Recipes.Any(r => r.HasNutrition);

    public RecipeComparisonViewModel(params Recipe[] recipes)
    {
        foreach (var recipe in recipes)
        {
            Recipes.Add(new RecipeComparisonItem(recipe));
        }
    }
}

public partial class RecipeComparisonItem : ObservableObject
{
    private readonly Recipe _recipe;

    public string Name { get; }
    public decimal TotalCost { get; }
    public decimal CostPerServing { get; }
    public decimal Yield { get; }
    public string YieldUnit { get; }
    public int IngredientCount { get; }
    public bool HasNutrition { get; }

    public string DifficultyDisplay => _recipe.Difficulty switch
    {
        DifficultyLevel.Easy => "Easy",
        DifficultyLevel.Medium => "Medium",
        DifficultyLevel.Hard => "Hard",
        DifficultyLevel.Expert => "Expert",
        _ => "Not Set"
    };

    public string PrepTimeDisplay => _recipe.PrepTimeMinutes.HasValue
        ? $"{_recipe.PrepTimeMinutes} minutes"
        : "N/A";

    public string CaloriesDisplay
    {
        get
        {
            var nutrition = _recipe.CalculatedNutrition;
            return nutrition.Calories > 0 ? $"{nutrition.Calories:F0} kcal" : "-";
        }
    }

    public string ProteinDisplay
    {
        get
        {
            var nutrition = _recipe.CalculatedNutrition;
            return nutrition.Protein > 0 ? $"{nutrition.Protein:F1}g" : "-";
        }
    }

    public string CarbsDisplay
    {
        get
        {
            var nutrition = _recipe.CalculatedNutrition;
            return nutrition.Carbohydrates > 0 ? $"{nutrition.Carbohydrates:F1}g" : "-";
        }
    }

    public string FatDisplay
    {
        get
        {
            var nutrition = _recipe.CalculatedNutrition;
            return nutrition.Fat > 0 ? $"{nutrition.Fat:F1}g" : "-";
        }
    }

    public RecipeComparisonItem(Recipe recipe)
    {
        _recipe = recipe;
        Name = recipe.Name;
        TotalCost = recipe.TotalCost;
        Yield = recipe.Yield;
        YieldUnit = recipe.YieldUnit;
        CostPerServing = recipe.Yield > 0 ? recipe.TotalCost / recipe.Yield : 0;
        IngredientCount = recipe.RecipeIngredients?.Count ?? 0;

        var nutrition = recipe.CalculatedNutrition;
        HasNutrition = nutrition.Calories > 0 ||
                      nutrition.Protein > 0 ||
                      nutrition.Carbohydrates > 0 ||
                      nutrition.Fat > 0;
    }
}
