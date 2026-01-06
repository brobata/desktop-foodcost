using Dfc.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Dfc.Core.Services;

public class ValidationFeedbackService : IValidationFeedbackService
{
    public ValidationFeedback ValidateIngredient(Ingredient ingredient)
    {
        var feedback = new ValidationFeedback();

        // Name validation
        if (string.IsNullOrWhiteSpace(ingredient.Name))
        {
            feedback.AddError("Name", "Ingredient name is required", "Enter a descriptive name for this ingredient");
        }
        else if (ingredient.Name.Length < 2)
        {
            feedback.AddError("Name", "Ingredient name must be at least 2 characters", "Use a more descriptive name");
        }
        else if (ingredient.Name.Length > 200)
        {
            feedback.AddError("Name", "Ingredient name is too long (max 200 characters)", "Shorten the ingredient name");
        }

        // Price validation
        if (ingredient.CurrentPrice <= 0)
        {
            feedback.AddWarning("Price", "Current price is $0.00 or not set", "Enter the current purchase price for accurate cost calculations");
        }
        else if (ingredient.CurrentPrice > 10000)
        {
            feedback.AddWarning("Price", $"Price of ${ingredient.CurrentPrice:F2} seems unusually high", "Verify this is the correct unit price (not case price)");
        }

        // Vendor validation
        if (string.IsNullOrWhiteSpace(ingredient.VendorName))
        {
            feedback.AddInfo("Vendor", "No vendor specified. Adding vendor information helps with price tracking and reordering");
        }

        // Category validation
        if (string.IsNullOrWhiteSpace(ingredient.Category))
        {
            feedback.AddInfo("Category", "No category assigned. Categorizing ingredients helps with organization and reporting");
        }

        // Case quantity validation
        if (ingredient.CaseQuantity <= 0)
        {
            feedback.AddWarning("Case Quantity", "Case quantity not set", "Set case quantity for accurate unit price calculations");
        }

        return feedback;
    }

    public ValidationFeedback ValidateRecipe(Recipe recipe)
    {
        var feedback = new ValidationFeedback();

        // Name validation
        if (string.IsNullOrWhiteSpace(recipe.Name))
        {
            feedback.AddError("Name", "Recipe name is required", "Enter a name for this recipe");
        }
        else if (recipe.Name.Length < 2)
        {
            feedback.AddError("Name", "Recipe name must be at least 2 characters", "Use a more descriptive name");
        }

        // Yield validation
        if (recipe.Yield <= 0)
        {
            feedback.AddError("Yield", "Recipe yield must be greater than zero", "Specify how many servings or portions this recipe makes");
        }

        // Yield unit validation
        if (string.IsNullOrWhiteSpace(recipe.YieldUnit))
        {
            feedback.AddError("Yield Unit", "Yield unit is required", "Specify the unit (e.g., 'servings', 'portions', 'oz')");
        }

        // Ingredients validation
        if (recipe.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
        {
            feedback.AddWarning("Ingredients", "Recipe has no ingredients", "Add at least one ingredient to calculate recipe cost");
        }
        else
        {
            var ingredientsWithoutCost = recipe.RecipeIngredients
                .Where(ri => ri.Ingredient?.CurrentPrice <= 0)
                .Count();

            if (ingredientsWithoutCost > 0)
            {
                feedback.AddWarning("Ingredients",
                    $"{ingredientsWithoutCost} ingredient(s) have no price set",
                    "Set prices for all ingredients for accurate cost calculation");
            }
        }

        // Prep time validation
        if (recipe.PrepTimeMinutes.HasValue && recipe.PrepTimeMinutes.Value <= 0)
        {
            feedback.AddWarning("Prep Time", "Prep time should be greater than zero", "Remove prep time or enter a valid value");
        }

        // Instructions validation
        if (string.IsNullOrWhiteSpace(recipe.Instructions))
        {
            feedback.AddInfo("Instructions", "No instructions provided. Adding instructions helps with recipe preparation");
        }

        // Category validation
        if (string.IsNullOrWhiteSpace(recipe.Category))
        {
            feedback.AddInfo("Category", "No category assigned. Categorizing recipes helps with organization");
        }

        // Difficulty validation
        if (recipe.Difficulty == Core.Enums.DifficultyLevel.NotSet)
        {
            feedback.AddInfo("Difficulty", "No difficulty rating set. Adding difficulty helps with planning and training");
        }

        return feedback;
    }

    public ValidationFeedback ValidateEntree(Entree entree)
    {
        var feedback = new ValidationFeedback();

        // Name validation
        if (string.IsNullOrWhiteSpace(entree.Name))
        {
            feedback.AddError("Name", "Entree name is required", "Enter a name for this entree");
        }
        else if (entree.Name.Length < 2)
        {
            feedback.AddError("Name", "Entree name must be at least 2 characters", "Use a more descriptive name");
        }

        // Menu price validation
        if (!entree.MenuPrice.HasValue || entree.MenuPrice <= 0)
        {
            feedback.AddWarning("Menu Price", "Menu price not set", "Enter the menu price to calculate food cost percentage");
        }
        else if (entree.MenuPrice > 1000)
        {
            feedback.AddWarning("Menu Price", $"Price of ${entree.MenuPrice:F2} seems unusually high", "Verify this is the correct menu price");
        }

        // Components validation
        var hasRecipes = entree.EntreeRecipes?.Any() == true;
        var hasIngredients = entree.EntreeIngredients?.Any() == true;

        if (!hasRecipes && !hasIngredients)
        {
            feedback.AddWarning("Components", "Entree has no recipes or ingredients", "Add recipes or direct ingredients to calculate cost");
        }

        // Food cost percentage validation
        if (entree.MenuPrice.HasValue && entree.TotalCost > 0)
        {
            var foodCostPercentage = entree.FoodCostPercentage;

            if (foodCostPercentage > 40)
            {
                feedback.AddWarning("Food Cost %",
                    $"Food cost percentage ({foodCostPercentage:F1}%) is high",
                    "Consider adjusting menu price or reducing ingredient costs to improve profitability");
            }
            else if (foodCostPercentage < 20)
            {
                feedback.AddInfo("Food Cost %",
                    $"Food cost percentage ({foodCostPercentage:F1}%) is low. Good profitability! Consider if there's room to increase portion sizes or quality");
            }
        }

        // Category validation
        if (string.IsNullOrWhiteSpace(entree.Category))
        {
            feedback.AddInfo("Category", "No category assigned. Categorizing entrees helps with menu planning");
        }

        // Photo validation
        if (string.IsNullOrWhiteSpace(entree.PhotoUrl) && (entree.Photos == null || !entree.Photos.Any()))
        {
            feedback.AddInfo("Photo", "No photo added. Adding photos helps with menu display and marketing");
        }

        return feedback;
    }

    public string FormatException(Exception exception)
    {
        return exception switch
        {
            DbUpdateException dbEx => FormatDbUpdateException(dbEx),
            ArgumentNullException argEx => $"❌ Missing required value: {argEx.ParamName}\n💡 Please provide a value for this field.",
            ArgumentException argEx => $"❌ Invalid value: {argEx.Message}\n💡 Check the format and try again.",
            InvalidOperationException opEx => $"❌ Operation failed: {opEx.Message}\n💡 This action cannot be performed in the current state.",
            UnauthorizedAccessException => "❌ Access denied\n💡 You don't have permission to perform this action.",
            TimeoutException => "❌ Operation timed out\n💡 The server is taking too long to respond. Please try again.",
            _ => $"❌ An error occurred: {exception.Message}\n💡 If this problem persists, please contact support."
        };
    }

    private string FormatDbUpdateException(DbUpdateException dbEx)
    {
        if (dbEx.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
        {
            return "❌ Duplicate entry\n💡 An item with this information already exists.";
        }

        if (dbEx.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
        {
            return "❌ Cannot save changes\n💡 This item is referenced by other records that must be updated first.";
        }

        return $"❌ Could not save changes\n💡 {dbEx.Message}";
    }
}
