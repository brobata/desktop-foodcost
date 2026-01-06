using System;

namespace Freecost.Core.Models;

/// <summary>
/// Represents a historical version of a recipe with all its properties frozen in time
/// </summary>
public class RecipeVersion
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? ChangeDescription { get; set; }

    // Snapshot of recipe data at this version
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public decimal Yield { get; set; }
    public string? YieldUnit { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public string? Difficulty { get; set; }
    public string? Tags { get; set; }
    public string? DietaryLabels { get; set; }
    public string? Notes { get; set; }

    // Nutritional information snapshot
    public decimal? Calories { get; set; }
    public decimal? Protein { get; set; }
    public decimal? Carbs { get; set; }
    public decimal? Fat { get; set; }
    public decimal? Fiber { get; set; }
    public decimal? Sugar { get; set; }
    public decimal? Sodium { get; set; }

    // JSON serialized snapshot of ingredients and allergens
    public string? IngredientsJson { get; set; }
    public string? AllergensJson { get; set; }

    // Calculated at version creation
    public decimal TotalCost { get; set; }

    // Navigation
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// Creates a version snapshot from a recipe
    /// </summary>
    public static RecipeVersion CreateFromRecipe(Recipe recipe, string? changedBy, string? changeDescription)
    {
        return new RecipeVersion
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = changedBy,
            ChangeDescription = changeDescription,
            Name = recipe.Name,
            Category = recipe.Category,
            Description = recipe.Description,
            Instructions = recipe.Instructions,
            Yield = recipe.Yield,
            YieldUnit = recipe.YieldUnit,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            Difficulty = recipe.Difficulty.ToString(),
            Tags = recipe.Tags,
            DietaryLabels = recipe.DietaryLabels,
            Notes = recipe.Notes,
            Calories = recipe.CalculatedNutrition.Calories,
            Protein = recipe.CalculatedNutrition.Protein,
            Carbs = recipe.CalculatedNutrition.Carbohydrates,
            Fat = recipe.CalculatedNutrition.Fat,
            Fiber = recipe.CalculatedNutrition.Fiber,
            Sugar = recipe.CalculatedNutrition.Sugar,
            Sodium = recipe.CalculatedNutrition.Sodium,
            TotalCost = recipe.TotalCost,
            // Serialize complex objects to JSON
            IngredientsJson = System.Text.Json.JsonSerializer.Serialize(recipe.RecipeIngredients),
            AllergensJson = System.Text.Json.JsonSerializer.Serialize(recipe.RecipeAllergens)
        };
    }
}
