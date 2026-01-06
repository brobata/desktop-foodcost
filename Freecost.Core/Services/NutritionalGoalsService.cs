using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class NutritionalGoalsService : INutritionalGoalsService
{
    private readonly string _goalsFilePath;
    private NutritionalGoals? _cachedGoals;

    public NutritionalGoalsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Freecost"
        );
        Directory.CreateDirectory(appDataPath);
        _goalsFilePath = Path.Combine(appDataPath, "nutritional-goals.json");
    }

    public async Task<NutritionalGoals> GetGoalsAsync()
    {
        if (_cachedGoals != null)
        {
            return _cachedGoals;
        }

        if (File.Exists(_goalsFilePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_goalsFilePath);
                _cachedGoals = JsonSerializer.Deserialize<NutritionalGoals>(json) ?? new NutritionalGoals();
                return _cachedGoals;
            }
            catch
            {
                // If deserialization fails, return empty goals
            }
        }

        _cachedGoals = new NutritionalGoals();
        return _cachedGoals;
    }

    public async Task SaveGoalsAsync(NutritionalGoals goals)
    {
        var json = JsonSerializer.Serialize(goals, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(_goalsFilePath, json);
        _cachedGoals = goals;
    }

    public List<NutritionalGoalComparison> CompareRecipeToGoals(Recipe recipe, NutritionalGoals goals)
    {
        var comparisons = new List<NutritionalGoalComparison>();
        var nutrition = recipe.CalculatedNutrition;

        if (goals.TargetCalories.HasValue)
        {
            comparisons.Add(CreateComparison("Calories", nutrition.Calories, goals.TargetCalories.Value, "kcal"));
        }

        if (goals.TargetProtein.HasValue)
        {
            comparisons.Add(CreateComparison("Protein", nutrition.Protein, goals.TargetProtein.Value, "g"));
        }

        if (goals.TargetCarbohydrates.HasValue)
        {
            comparisons.Add(CreateComparison("Carbs", nutrition.Carbohydrates, goals.TargetCarbohydrates.Value, "g"));
        }

        if (goals.TargetFat.HasValue)
        {
            comparisons.Add(CreateComparison("Fat", nutrition.Fat, goals.TargetFat.Value, "g"));
        }

        if (goals.TargetFiber.HasValue)
        {
            comparisons.Add(CreateComparison("Fiber", nutrition.Fiber, goals.TargetFiber.Value, "g"));
        }

        if (goals.TargetSugar.HasValue)
        {
            comparisons.Add(CreateComparison("Sugar", nutrition.Sugar, goals.TargetSugar.Value, "g"));
        }

        if (goals.TargetSodium.HasValue)
        {
            comparisons.Add(CreateComparison("Sodium", nutrition.Sodium, goals.TargetSodium.Value, "mg"));
        }

        return comparisons;
    }

    private NutritionalGoalComparison CreateComparison(string nutrient, decimal actual, decimal target, string unit)
    {
        var percentOfGoal = target > 0 ? (actual / target) * 100 : 0;

        var compliance = percentOfGoal switch
        {
            < 50 => ComplianceLevel.Under,
            >= 50 and <= 100 => ComplianceLevel.Good,
            > 100 and <= 120 => ComplianceLevel.Meets,
            _ => ComplianceLevel.Exceeds
        };

        return new NutritionalGoalComparison
        {
            Nutrient = $"{nutrient} ({unit})",
            ActualValue = actual,
            TargetValue = target,
            PercentOfGoal = percentOfGoal,
            Compliance = compliance
        };
    }
}
