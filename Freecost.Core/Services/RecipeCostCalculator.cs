// Location: Freecost.Core/Services/RecipeCostCalculator.cs
// Action: REPLACE entire file

using Freecost.Core.Enums;
using Freecost.Core.Helpers;
using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class RecipeCostCalculator : IRecipeCostCalculator
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IUniversalConversionService _conversionService;

    public RecipeCostCalculator(
        IIngredientRepository ingredientRepository,
        IUniversalConversionService conversionService)
    {
        _ingredientRepository = ingredientRepository;
        _conversionService = conversionService;
    }

    public async Task<decimal> CalculateIngredientCostAsync(RecipeIngredient recipeIngredient)
    {
        // If ingredient is not matched to database, return 0
        if (!recipeIngredient.IngredientId.HasValue) return 0m;

        var ingredient = await _ingredientRepository.GetByIdAsync(recipeIngredient.IngredientId.Value);
        if (ingredient == null) return 0m;
        if (ingredient.CurrentPrice <= 0) return 0m;

        try
        {
            return CalculateCostInternal(recipeIngredient, ingredient, out _);
        }
        catch
        {
            return 0m;
        }
    }

    public async Task<decimal> CalculateRecipeTotalCostAsync(Recipe recipe)
    {
        if (recipe.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
            return 0m;

        decimal totalCost = 0m;

        foreach (var recipeIngredient in recipe.RecipeIngredients)
        {
            // Handle missing ingredients (not yet added to database)
            if (!recipeIngredient.IngredientId.HasValue)
            {
                recipeIngredient.CalculatedCost = 0m;
                recipeIngredient.HasValidCost = false;
                recipeIngredient.CostWarningMessage = $"Ingredient '{recipeIngredient.UnmatchedIngredientName}' needs to be added to ingredients list";
                continue;
            }

            var ingredient = await _ingredientRepository.GetByIdAsync(recipeIngredient.IngredientId.Value);
            if (ingredient == null)
            {
                recipeIngredient.CalculatedCost = 0m;
                recipeIngredient.HasValidCost = false;
                recipeIngredient.CostWarningMessage = "Ingredient not found";
                continue;
            }

            if (ingredient.CurrentPrice <= 0)
            {
                recipeIngredient.CalculatedCost = 0m;
                recipeIngredient.HasValidCost = false;
                recipeIngredient.CostWarningMessage = $"No price set for {ingredient.Name}";
                continue;
            }

            try
            {
                decimal cost = CalculateCostInternal(recipeIngredient, ingredient, out string? warning);
                recipeIngredient.CalculatedCost = cost;
                recipeIngredient.HasValidCost = warning == null;
                recipeIngredient.CostWarningMessage = warning;
                totalCost += cost;
            }
            catch (Exception ex)
            {
                recipeIngredient.CalculatedCost = 0m;
                recipeIngredient.HasValidCost = false;
                recipeIngredient.CostWarningMessage = $"Error: {ex.Message}";
            }
        }

        return totalCost;
    }

    public async Task<decimal> CalculateCostPerServingAsync(Recipe recipe)
    {
        if (recipe.Yield <= 0)
            return 0m;

        var totalCost = await CalculateRecipeTotalCostAsync(recipe);
        return totalCost / recipe.Yield;
    }

    public async Task<Dictionary<Guid, RecipeCostSummary>> CalculateBatchCostsAsync(IEnumerable<Recipe> recipes)
    {
        var results = new Dictionary<Guid, RecipeCostSummary>();

        foreach (var recipe in recipes)
        {
            var totalCost = await CalculateRecipeTotalCostAsync(recipe);
            var costPerServing = recipe.Yield > 0 ? totalCost / recipe.Yield : 0m;

            results[recipe.Id] = new RecipeCostSummary
            {
                RecipeId = recipe.Id,
                RecipeName = recipe.Name,
                TotalCost = totalCost,
                CostPerServing = costPerServing,
                Yield = recipe.Yield,
                YieldUnit = recipe.YieldUnit,
                IngredientCount = recipe.RecipeIngredients?.Count ?? 0
            };
        }

        return results;
    }

    private decimal CalculateCostInternal(RecipeIngredient recipeIngredient, Ingredient ingredient, out string? warningMessage)
    {
        warningMessage = null;

        // Same unit - direct calculation
        if (recipeIngredient.Unit == ingredient.Unit)
        {
            return recipeIngredient.Quantity * ingredient.CurrentPrice;
        }

        // Try universal conversion (3-layer fallback: ingredient-specific → USDA/built-in → standard)
        var convertedQuantity = _conversionService.ConvertAsync(
            recipeIngredient.Quantity,
            recipeIngredient.Unit,
            ingredient.Unit,
            ingredient.Id,
            ingredient.Name,
            ingredient.LocationId).GetAwaiter().GetResult();

        if (convertedQuantity.HasValue)
        {
            // Conversion succeeded
            var source = _conversionService.GetConversionSourceAsync(
                recipeIngredient.Unit,
                ingredient.Unit,
                ingredient.Id,
                ingredient.Name,
                ingredient.LocationId).GetAwaiter().GetResult();

            // Optionally add info message about conversion source
            // warningMessage = $"Using {source} conversion";

            return convertedQuantity.Value * ingredient.CurrentPrice;
        }

        // Fallback: Try legacy alternate unit conversion for backwards compatibility
        if (ingredient.UseAlternateUnit &&
            ingredient.AlternateUnit.HasValue &&
            recipeIngredient.Unit == ingredient.AlternateUnit.Value)
        {
            if (!ingredient.AlternateConversionQuantity.HasValue ||
                !ingredient.AlternateConversionUnit.HasValue)
            {
                warningMessage = $"Alternate unit conversion not configured for {ingredient.Name}";
                return 0m;
            }

            // Convert recipe quantity using alternate conversion
            decimal quantityInConversionUnit = recipeIngredient.Quantity * ingredient.AlternateConversionQuantity.Value;

            // Convert to purchase unit
            if (!UnitConverter.CanConvert(ingredient.AlternateConversionUnit.Value, ingredient.Unit))
            {
                warningMessage = $"Cannot convert {ingredient.AlternateConversionUnit} to {ingredient.Unit}";
                return 0m;
            }

            decimal quantityInPurchaseUnit = UnitConverter.Convert(
                quantityInConversionUnit,
                ingredient.AlternateConversionUnit.Value,
                ingredient.Unit
            );

            return quantityInPurchaseUnit * ingredient.CurrentPrice;
        }

        // Cannot convert
        warningMessage = $"{ingredient.Name}: Cannot convert {recipeIngredient.Unit} to {ingredient.Unit}. " +
                        $"Add conversion data to fix.";
        return 0m;
    }
}

public class RecipeCostSummary
{
    public Guid RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal CostPerServing { get; set; }
    public decimal Yield { get; set; }
    public string YieldUnit { get; set; } = string.Empty;
    public int IngredientCount { get; set; }
}