using Freecost.Core.Models;

namespace Freecost.Core.Services;

public interface IExcelExportService
{
    /// <summary>
    /// Exports ingredients to an Excel file
    /// </summary>
    Task<string> ExportIngredientsToExcelAsync(List<Ingredient> ingredients, string filePath);

    /// <summary>
    /// Exports recipes to an Excel file with ingredients
    /// </summary>
    Task<string> ExportRecipesToExcelAsync(List<Recipe> recipes, string filePath);

    /// <summary>
    /// Exports entrees to an Excel file with all components
    /// </summary>
    Task<string> ExportEntreesToExcelAsync(List<Entree> entrees, string filePath);

    /// <summary>
    /// Imports recipes from an Excel file
    /// </summary>
    Task<List<Recipe>> ImportRecipesFromExcelAsync(string filePath);

    /// <summary>
    /// Imports entrees from an Excel file
    /// </summary>
    Task<List<Entree>> ImportEntreesFromExcelAsync(string filePath);
}
