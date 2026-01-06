using Freecost.Core.Constants;
using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Core.Services;

public class ValidationService : IValidationService
{
    // Valid yield units for recipes - common units for recipe output
    private static readonly HashSet<string> ValidYieldUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        // Servings and portions (MOST COMMON - recommended)
        "serving", "servings", "portion", "portions",
        // Count units
        "piece", "pieces", "each", "count", "dozen", "item", "items",
        // Volume - US
        "teaspoon", "tsp", "tablespoon", "tbsp", "fluid ounce", "fl oz", "oz",
        "cup", "cups", "pint", "pints", "quart", "quarts", "gallon", "gallons",
        // Volume - Metric
        "milliliter", "ml", "liter", "l", "liters",
        // Weight - US
        "ounce", "pound", "lb", "lbs", "pounds",
        // Weight - Metric
        "gram", "g", "grams", "kilogram", "kg", "kilograms",
        // Bakery specific
        "loaf", "loaves", "roll", "rolls", "slice", "slices", "cookie", "cookies",
        // Restaurant specific
        "plate", "plates", "bowl", "bowls", "appetizer", "appetizers",
        "batch", "batches", "pan", "pans", "tray", "trays",
        "container", "containers", "bag", "bags"
    };

    // Recommended yield units (most appropriate for restaurant recipes)
    private static readonly HashSet<string> RecommendedYieldUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        "servings", "portions", "pieces", "plates", "bowls"
    };

    public ValidationResult ValidateRecipe(Recipe recipe)
    {
        var result = new ValidationResult();

        ValidateRecipeBasicFields(recipe, result);
        ValidateRecipeYield(recipe, result);
        ValidateRecipeInstructions(recipe, result);
        ValidateRecipeIngredients(recipe, result);
        ValidateRecipeTime(recipe, result);
        ValidateRecipeOrganization(recipe, result);
        ValidateRecipeNutrition(recipe, result);

        return result;
    }

    /// <summary>
    /// Validates basic recipe fields like name
    /// </summary>
    private void ValidateRecipeBasicFields(Recipe recipe, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(recipe.Name))
        {
            result.AddError(nameof(recipe.Name), "Recipe name is required. Please enter a descriptive name for this recipe (e.g., 'Chocolate Chip Cookies', 'Caesar Salad').");
        }
        else if (recipe.Name.Length < ValidationConstants.NameLimits.MIN_LENGTH)
        {
            result.AddError(nameof(recipe.Name), $"Recipe name must be at least {ValidationConstants.NameLimits.MIN_LENGTH} characters long.");
        }
        else if (recipe.Name.Length > ValidationConstants.NameLimits.MAX_LENGTH)
        {
            result.AddError(nameof(recipe.Name), $"Recipe name cannot exceed {ValidationConstants.NameLimits.MAX_LENGTH} characters.");
        }
    }

    /// <summary>
    /// Validates recipe yield and yield unit
    /// </summary>
    private void ValidateRecipeYield(Recipe recipe, ValidationResult result)
    {
        if (recipe.Yield <= 0)
        {
            result.AddError(nameof(recipe.Yield), "Yield must be greater than 0. Enter the number of servings, portions, or units this recipe produces.");
        }
        else if (recipe.Yield > ValidationConstants.YieldLimits.MAX_QUANTITY)
        {
            result.AddWarning(nameof(recipe.Yield), $"Yield seems unusually large. Please verify this recipe produces {recipe.Yield} {recipe.YieldUnit ?? "units"}.");
        }

        if (string.IsNullOrWhiteSpace(recipe.YieldUnit))
        {
            result.AddError(nameof(recipe.YieldUnit), "Yield unit is required. Specify what the recipe produces (e.g., 'servings', 'portions', 'pieces').");
        }
        else
        {
            var trimmedUnit = recipe.YieldUnit.Trim();

            if (!ValidYieldUnits.Contains(trimmedUnit))
            {
                result.AddError(nameof(recipe.YieldUnit),
                    $"'{recipe.YieldUnit}' is not a recognized yield unit.\n\n" +
                    "Recommended units: servings, portions, pieces, plates, bowls\n" +
                    "Also valid: cups, lbs, kg, loaves, slices, batches, pans\n\n" +
                    "Using standard units ensures consistency across recipes and accurate cost-per-serving calculations.");
            }
            else if (!RecommendedYieldUnits.Contains(trimmedUnit))
            {
                // Valid but not recommended - provide helpful guidance
                var suggestions = GetYieldUnitSuggestion(recipe.YieldUnit, recipe.Yield, trimmedUnit);
                result.AddInfo(nameof(recipe.YieldUnit), suggestions);
            }

            // Check for yield unit consistency with yield amount
            if (recipe.Yield < 1 && (trimmedUnit.Contains("serving") || trimmedUnit.Contains("portion") || trimmedUnit.Contains("piece")))
            {
                result.AddWarning(nameof(recipe.Yield),
                    $"Yield of {recipe.Yield} {recipe.YieldUnit} seems unusual - servings/portions are typically whole numbers (1, 2, 3, etc.). " +
                    "Did you mean to use a different unit like 'cups' or 'lbs'?");
            }

            // Check for very large yields with small units
            if (recipe.Yield > ValidationConstants.YieldLimits.HIGH_YIELD_WARNING_THRESHOLD && RecommendedYieldUnits.Contains(trimmedUnit))
            {
                result.AddWarning(nameof(recipe.Yield),
                    $"This recipe yields {recipe.Yield} {recipe.YieldUnit} which is quite large. " +
                    "Verify this is correct, or consider if this should be measured in batches (e.g., '5 batches of 20 servings').");
            }
        }
    }

    /// <summary>
    /// Gets a helpful suggestion for non-recommended but valid yield units
    /// </summary>
    private string GetYieldUnitSuggestion(string yieldUnit, decimal yield, string trimmedUnit)
    {
        return trimmedUnit.ToLowerInvariant() switch
        {
            var u when u.Contains("cup") || u.Contains("oz") || u.Contains("lb") || u.Contains("kg") || u.Contains("gram") =>
                "Weight/volume units are valid but consider using 'servings' or 'portions' for better menu planning. " +
                $"If this recipe makes {yield} {yieldUnit} total, how many servings does that create?",

            var u when u.Contains("batch") || u.Contains("pan") || u.Contains("tray") =>
                $"'{yieldUnit}' is valid for bulk preparation. For better cost analysis, also note how many servings per {yieldUnit}.",

            var u when u.Contains("loaf") || u.Contains("loaves") || u.Contains("roll") || u.Contains("slice") =>
                $"'{yieldUnit}' is perfect for bakery items. This helps track per-unit costs.",

            var u when u.Contains("each") || u.Contains("count") || u.Contains("item") =>
                "Consider using 'pieces' instead of '" + yieldUnit + "' for clarity in recipe yields.",

            _ => $"'{yieldUnit}' is valid. For better standardization, consider using 'servings', 'portions', or 'pieces'."
        };
    }

    /// <summary>
    /// Validates recipe instructions
    /// </summary>
    private void ValidateRecipeInstructions(Recipe recipe, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(recipe.Instructions))
        {
            result.AddWarning(nameof(recipe.Instructions), "No instructions provided. Adding preparation steps helps maintain consistency and train new staff.");
        }
        else if (recipe.Instructions.Length < ValidationConstants.ContentLimits.MIN_INSTRUCTION_LENGTH)
        {
            result.AddWarning(nameof(recipe.Instructions), "Instructions seem very brief. Consider adding more detailed preparation steps.");
        }
    }

    /// <summary>
    /// Validates recipe ingredients list
    /// </summary>
    private void ValidateRecipeIngredients(Recipe recipe, ValidationResult result)
    {
        if (recipe.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
        {
            result.AddError(nameof(recipe.RecipeIngredients), "Recipe must have at least one ingredient. Add ingredients to calculate cost and provide recipe details.");
            return;
        }

        // Check for unpriced ingredients with details
        var unpricedIngredients = recipe.RecipeIngredients
            .Where(ri => ri.Ingredient != null && ri.Ingredient.CurrentPrice <= 0)
            .Select(ri => ri.Ingredient!.Name)
            .ToList();

        if (unpricedIngredients.Any())
        {
            var ingredientList = string.Join(", ", unpricedIngredients.Take(3));
            if (unpricedIngredients.Count > 3)
                ingredientList += $" and {unpricedIngredients.Count - 3} more";

            result.AddWarning("Ingredients",
                $"The following ingredient(s) have no price set: {ingredientList}. Update ingredient prices for accurate cost calculations.");
        }

        // Check for zero quantity ingredients
        var zeroQuantityIngredients = recipe.RecipeIngredients
            .Where(ri => ri.Quantity <= 0)
            .Select(ri => ri.Ingredient?.Name ?? "Unknown")
            .ToList();

        if (zeroQuantityIngredients.Any())
        {
            result.AddError("Ingredients",
                $"Ingredient quantity must be greater than 0 for: {string.Join(", ", zeroQuantityIngredients)}");
        }
    }

    /// <summary>
    /// Validates recipe time fields
    /// </summary>
    private void ValidateRecipeTime(Recipe recipe, ValidationResult result)
    {
        if (!recipe.PrepTimeMinutes.HasValue)
        {
            result.AddWarning(nameof(recipe.PrepTimeMinutes), "Prep time not specified. Adding prep time helps with scheduling and labor cost planning.");
        }
        else if (recipe.PrepTimeMinutes.Value <= 0)
        {
            result.AddError(nameof(recipe.PrepTimeMinutes), "Prep time must be greater than 0 minutes.");
        }
        else if (recipe.PrepTimeMinutes.Value > ValidationConstants.TimeLimits.MAX_PREP_TIME_MINUTES)
        {
            result.AddWarning(nameof(recipe.PrepTimeMinutes), "Prep time exceeds 24 hours. Please verify this is correct.");
        }
    }

    /// <summary>
    /// Validates recipe organization fields (category, difficulty, tags)
    /// </summary>
    private void ValidateRecipeOrganization(Recipe recipe, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(recipe.Category))
        {
            result.AddWarning(nameof(recipe.Category), "No category assigned. Categorizing recipes helps with organization and menu planning.");
        }

        if (recipe.Difficulty == Enums.DifficultyLevel.NotSet)
        {
            result.AddWarning(nameof(recipe.Difficulty), "Difficulty level not set. This helps staff know skill requirements (Easy, Medium, Hard, or Expert).");
        }

        if (string.IsNullOrWhiteSpace(recipe.Tags))
        {
            result.AddWarning(nameof(recipe.Tags), "No tags added. Tags improve searchability (e.g., 'vegetarian', 'gluten-free', 'quick').");
        }
    }

    /// <summary>
    /// Validates recipe nutritional information
    /// </summary>
    private void ValidateRecipeNutrition(Recipe recipe, ValidationResult result)
    {
        var nutrition = recipe.CalculatedNutrition;

        // Check for extremely high nutritional values per serving (likely data entry errors)
        if (nutrition.Calories > ValidationConstants.NutritionLimits.CALORIES_EXTREME_HIGH)
        {
            result.AddWarning("Nutrition", $"Calories per serving ({nutrition.Calories}) seems extremely high. Please verify ingredient quantities and serving size.");
        }
        else if (nutrition.Calories > ValidationConstants.NutritionLimits.CALORIES_HIGH)
        {
            result.AddWarning("Nutrition", $"This is a high-calorie recipe ({nutrition.Calories} cal/serving). Consider noting this for menu labeling.");
        }

        if (nutrition.Protein > ValidationConstants.NutritionLimits.PROTEIN_HIGH)
        {
            result.AddWarning("Nutrition", $"Protein per serving ({nutrition.Protein}g) seems unusually high. Please verify ingredient quantities.");
        }

        if (nutrition.Sodium > ValidationConstants.NutritionLimits.SODIUM_DAILY_LIMIT)
        {
            result.AddWarning("Nutrition", $"Sodium content ({nutrition.Sodium}mg/serving) exceeds daily recommended limit. Consider for dietary labeling.");
        }

        if (nutrition.Sugar > ValidationConstants.NutritionLimits.SUGAR_HIGH)
        {
            result.AddWarning("Nutrition", $"Sugar content ({nutrition.Sugar}g/serving) is very high. Consider for dietary labeling.");
        }
    }

    public ValidationResult ValidateIngredient(Ingredient ingredient)
    {
        var result = new ValidationResult();

        // Required fields with detailed messages
        if (string.IsNullOrWhiteSpace(ingredient.Name))
        {
            result.AddError(nameof(ingredient.Name), "Ingredient name is required. Please enter a descriptive name (e.g., 'All-Purpose Flour', 'Extra Virgin Olive Oil').");
        }
        else if (ingredient.Name.Length < ValidationConstants.NameLimits.MIN_LENGTH)
        {
            result.AddError(nameof(ingredient.Name), $"Ingredient name must be at least {ValidationConstants.NameLimits.MIN_LENGTH} characters long.");
        }
        else if (ingredient.Name.Length > ValidationConstants.NameLimits.MAX_LENGTH)
        {
            result.AddError(nameof(ingredient.Name), $"Ingredient name cannot exceed {ValidationConstants.NameLimits.MAX_LENGTH} characters.");
        }

        // Price validation
        if (ingredient.CurrentPrice < 0)
        {
            result.AddError(nameof(ingredient.CurrentPrice), "Price cannot be negative. Enter the current purchase price per case.");
        }
        else if (ingredient.CurrentPrice == 0)
        {
            result.AddWarning(nameof(ingredient.CurrentPrice), "No price set. Recipes using this ingredient will show $0.00 cost until you add a price.");
        }
        else if (ingredient.CurrentPrice > ValidationConstants.PriceLimits.MAX_INGREDIENT_PRICE)
        {
            result.AddWarning(nameof(ingredient.CurrentPrice), $"Price per case (${ingredient.CurrentPrice:F2}) seems unusually high. Please verify this is correct.");
        }

        // Case quantity validation
        if (ingredient.CaseQuantity <= 0)
        {
            result.AddError(nameof(ingredient.CaseQuantity), "Case quantity must be greater than 0. This is the amount you receive per case/package.");
        }
        else if (ingredient.CaseQuantity > ValidationConstants.QuantityLimits.MAX_CASE_QUANTITY)
        {
            result.AddWarning(nameof(ingredient.CaseQuantity), $"Case quantity ({ingredient.CaseQuantity}) seems very large. Please verify this is correct.");
        }

        // Vendor information
        if (string.IsNullOrWhiteSpace(ingredient.VendorName))
        {
            result.AddWarning(nameof(ingredient.VendorName), "No vendor specified. Adding vendor information helps with ordering and price tracking.");
        }

        // Category
        if (string.IsNullOrWhiteSpace(ingredient.Category))
        {
            result.AddWarning(nameof(ingredient.Category), "No category assigned. Categorizing ingredients helps with organization and reporting.");
        }

        // Alternate unit conversion validation
        if (ingredient.UseAlternateUnit)
        {
            if (!ingredient.AlternateConversionQuantity.HasValue || !ingredient.AlternateConversionUnit.HasValue)
            {
                result.AddError("AlternateUnit", "Alternate unit conversion is incomplete. Please specify both conversion quantity and unit, or disable alternate units.");
            }
            else if (ingredient.AlternateConversionQuantity.Value <= 0)
            {
                result.AddError("AlternateUnit", "Alternate conversion quantity must be greater than 0.");
            }
            else
            {
                // Validate conversion makes sense
                var ratio = ingredient.CaseQuantity / ingredient.AlternateConversionQuantity.Value;
                if (ratio < ValidationConstants.ConversionLimits.MIN_RATIO || ratio > ValidationConstants.ConversionLimits.MAX_RATIO)
                {
                    result.AddWarning("AlternateUnit",
                        $"Conversion ratio seems unusual ({ingredient.CaseQuantity} {ingredient.Unit} = {ingredient.AlternateConversionQuantity} {ingredient.AlternateConversionUnit}). Please verify this is correct.");
                }
            }
        }

        // Nutritional information
        if (!ingredient.CaloriesPerUnit.HasValue)
        {
            result.AddWarning("Nutrition", "No nutritional information provided. Adding this helps calculate recipe nutrition automatically.");
        }

        return result;
    }

    public ValidationResult ValidateEntree(Entree entree)
    {
        var result = new ValidationResult();

        // Required fields with detailed messages
        if (string.IsNullOrWhiteSpace(entree.Name))
        {
            result.AddError(nameof(entree.Name), "Entree name is required. Please enter a menu item name (e.g., 'Grilled Salmon Plate', 'Classic Burger').");
        }
        else if (entree.Name.Length < ValidationConstants.NameLimits.MIN_LENGTH)
        {
            result.AddError(nameof(entree.Name), $"Entree name must be at least {ValidationConstants.NameLimits.MIN_LENGTH} characters long.");
        }
        else if (entree.Name.Length > ValidationConstants.NameLimits.MAX_LENGTH)
        {
            result.AddError(nameof(entree.Name), $"Entree name cannot exceed {ValidationConstants.NameLimits.MAX_LENGTH} characters.");
        }

        // Recipe and ingredient composition
        bool hasRecipes = entree.EntreeRecipes != null && entree.EntreeRecipes.Any();
        bool hasIngredients = entree.EntreeIngredients != null && entree.EntreeIngredients.Any();

        if (!hasRecipes && !hasIngredients)
        {
            result.AddError(nameof(entree.EntreeRecipes), "Entree must include at least one recipe component or ingredient. Add recipes or ingredients to build this menu item and calculate costs.");
        }

        // Validate recipe quantities
        if (hasRecipes)
        {
            // Check for zero portion quantities
            var zeroQuantities = entree.EntreeRecipes
                .Where(er => er.Quantity <= 0)
                .Select(er => er.Recipe?.Name ?? "Unknown")
                .ToList();

            if (zeroQuantities.Any())
            {
                result.AddError("EntreeRecipes",
                    $"Recipe quantity must be greater than 0 for: {string.Join(", ", zeroQuantities)}");
            }
        }

        // Validate ingredient quantities
        if (hasIngredients)
        {
            // Check for zero quantities
            var zeroIngredientQuantities = entree.EntreeIngredients
                .Where(ei => ei.Quantity <= 0)
                .Select(ei => ei.Ingredient?.Name ?? "Unknown")
                .ToList();

            if (zeroIngredientQuantities.Any())
            {
                result.AddError("EntreeIngredients",
                    $"Ingredient quantity must be greater than 0 for: {string.Join(", ", zeroIngredientQuantities)}");
            }
        }

        // Description
        if (string.IsNullOrWhiteSpace(entree.Description))
        {
            result.AddWarning(nameof(entree.Description), "No description provided. Adding a menu description helps with marketing and customer communication.");
        }
        else if (entree.Description.Length < ValidationConstants.ContentLimits.MIN_DESCRIPTION_LENGTH)
        {
            result.AddWarning(nameof(entree.Description), "Description seems very brief. Consider adding more detail for menu presentation.");
        }

        // Category
        if (string.IsNullOrWhiteSpace(entree.Category))
        {
            result.AddWarning(nameof(entree.Category), "No category assigned (e.g., 'Appetizer', 'Main Course', 'Dessert'). This helps with menu organization.");
        }

        // Photo
        if (string.IsNullOrWhiteSpace(entree.PhotoUrl))
        {
            result.AddWarning(nameof(entree.PhotoUrl), "No photo uploaded. High-quality photos help with marketing and menu presentation.");
        }

        // Menu price validation
        if (!entree.MenuPrice.HasValue || entree.MenuPrice <= 0)
        {
            result.AddWarning(nameof(entree.MenuPrice), "No menu price set. Add a selling price to calculate food cost percentage and profit margins.");
        }
        else
        {
            // Calculate food cost percentage if we have recipe costs
            if (entree.EntreeRecipes?.Any() == true)
            {
                var totalCost = entree.EntreeRecipes.Sum(er =>
                {
                    if (er.Recipe?.RecipeIngredients == null) return 0m;
                    var recipeCost = er.Recipe.RecipeIngredients.Sum(ri =>
                        ri.Ingredient != null ? (ri.Quantity * ri.Ingredient.CurrentPrice / ri.Ingredient.CaseQuantity) : 0m);
                    return recipeCost / (er.Recipe.Yield > 0 ? er.Recipe.Yield : 1) * er.Quantity;
                });

                if (totalCost > 0)
                {
                    var foodCostPercent = (totalCost / entree.MenuPrice.Value) * 100;

                    if (foodCostPercent > ValidationConstants.FoodCostLimits.HIGH_THRESHOLD_PERCENT)
                    {
                        result.AddWarning("Profitability",
                            $"Food cost is {foodCostPercent:F1}% (${totalCost:F2} cost / ${entree.MenuPrice.Value:F2} price). " +
                            $"Industry standard is {ValidationConstants.FoodCostLimits.INDUSTRY_MIN_PERCENT}-{ValidationConstants.FoodCostLimits.INDUSTRY_MAX_PERCENT}%. " +
                            "Consider adjusting recipe portions or menu price.");
                    }
                    else if (foodCostPercent < ValidationConstants.FoodCostLimits.LOW_THRESHOLD_PERCENT)
                    {
                        result.AddWarning("Profitability",
                            $"Food cost is only {foodCostPercent:F1}%. You may have room to improve portions or reduce the menu price for competitive advantage.");
                    }
                }
            }

            // Price reasonableness check
            if (entree.MenuPrice > ValidationConstants.PriceLimits.MAX_MENU_PRICE)
            {
                result.AddWarning(nameof(entree.MenuPrice), $"Menu price (${entree.MenuPrice:F2}) seems very high. Please verify this is correct.");
            }
        }

        return result;
    }
}
