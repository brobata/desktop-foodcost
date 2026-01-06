using Dfc.Core.Helpers;
using Dfc.Core.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Dfc.Core.Services;

public class ExcelExportService : IExcelExportService
{
    public async Task<string> ExportIngredientsToExcelAsync(List<Ingredient> ingredients, string filePath)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Ingredients");

        // Header row
        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Category";
        worksheet.Cells[1, 3].Value = "Current Price";
        worksheet.Cells[1, 4].Value = "Unit";
        worksheet.Cells[1, 5].Value = "Case Quantity";
        worksheet.Cells[1, 6].Value = "Vendor Name";
        worksheet.Cells[1, 7].Value = "Vendor SKU";

        // Style header
        using (var range = worksheet.Cells[1, 1, 1, 7])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(122, 181, 29)); // #7AB51D
            range.Style.Font.Color.SetColor(Color.White);
        }

        // Data rows
        int row = 2;
        foreach (var ingredient in ingredients.OrderBy(i => i.Category).ThenBy(i => i.Name))
        {
            worksheet.Cells[row, 1].Value = ingredient.Name;
            worksheet.Cells[row, 2].Value = ingredient.Category;
            worksheet.Cells[row, 3].Value = ingredient.CurrentPrice;
            worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
            worksheet.Cells[row, 4].Value = UnitConverter.GetAbbreviation(ingredient.Unit);
            worksheet.Cells[row, 5].Value = ingredient.CaseQuantity;
            worksheet.Cells[row, 6].Value = ingredient.VendorName;
            worksheet.Cells[row, 7].Value = ingredient.VendorSku;
            row++;
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Save
        await package.SaveAsAsync(new FileInfo(filePath));
        return filePath;
    }

    public async Task<string> ExportRecipesToExcelAsync(List<Recipe> recipes, string filePath)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Recipes");

        // Header row
        worksheet.Cells[1, 1].Value = "Recipe Name";
        worksheet.Cells[1, 2].Value = "Description";
        worksheet.Cells[1, 3].Value = "Yield";
        worksheet.Cells[1, 4].Value = "Yield Unit";
        worksheet.Cells[1, 5].Value = "Prep Time (min)";
        worksheet.Cells[1, 6].Value = "Total Cost";
        worksheet.Cells[1, 7].Value = "Cost Per Serving";

        // Style header
        using (var range = worksheet.Cells[1, 1, 1, 7])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(122, 181, 29));
            range.Style.Font.Color.SetColor(Color.White);
        }

        // Data rows
        int row = 2;
        foreach (var recipe in recipes.OrderBy(r => r.Name))
        {
            worksheet.Cells[row, 1].Value = recipe.Name;
            worksheet.Cells[row, 2].Value = recipe.Description;
            worksheet.Cells[row, 3].Value = recipe.Yield;
            worksheet.Cells[row, 4].Value = recipe.YieldUnit;
            worksheet.Cells[row, 5].Value = recipe.PrepTimeMinutes;
            worksheet.Cells[row, 6].Value = recipe.TotalCost;
            worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
            worksheet.Cells[row, 7].Value = recipe.CostPerServing;
            worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";
            row++;
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Save
        await package.SaveAsAsync(new FileInfo(filePath));
        return filePath;
    }

    public async Task<string> ExportEntreesToExcelAsync(List<Entree> entrees, string filePath)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Entrees");

        // Header row
        worksheet.Cells[1, 1].Value = "Entree Name";
        worksheet.Cells[1, 2].Value = "Description";
        worksheet.Cells[1, 3].Value = "Menu Price";
        worksheet.Cells[1, 4].Value = "Total Cost";
        worksheet.Cells[1, 5].Value = "Food Cost %";

        // Style header
        using (var range = worksheet.Cells[1, 1, 1, 5])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(122, 181, 29));
            range.Style.Font.Color.SetColor(Color.White);
        }

        // Data rows
        int row = 2;
        foreach (var entree in entrees.OrderBy(e => e.Name))
        {
            worksheet.Cells[row, 1].Value = entree.Name;
            worksheet.Cells[row, 2].Value = entree.Description;
            worksheet.Cells[row, 3].Value = entree.MenuPrice;
            worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
            worksheet.Cells[row, 4].Value = entree.TotalCost;
            worksheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";

            var foodCostPercent = entree.MenuPrice > 0 ? (entree.TotalCost / entree.MenuPrice) * 100 : 0;
            worksheet.Cells[row, 5].Value = foodCostPercent;
            worksheet.Cells[row, 5].Style.Numberformat.Format = "0.0%";
            row++;
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Save
        await package.SaveAsAsync(new FileInfo(filePath));
        return filePath;
    }

    public Task<List<Recipe>> ImportRecipesFromExcelAsync(string filePath)
    {
        var recipes = new List<Recipe>();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets["Recipes"];

        if (worksheet == null)
        {
            throw new Exception("Recipes worksheet not found in the Excel file.");
        }

        int rowCount = worksheet.Dimension?.Rows ?? 0;

        // Start from row 2 (skip header)
        for (int row = 2; row <= rowCount; row++)
        {
            var name = worksheet.Cells[row, 1].Value?.ToString();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty,
                Yield = decimal.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out var yieldVal) ? yieldVal : 0,
                YieldUnit = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty,
                PrepTimeMinutes = int.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out var prepTime) ? prepTime : 0,
                RecipeIngredients = new List<RecipeIngredient>(),
                CreatedAt = DateTime.UtcNow
            };

            recipes.Add(recipe);
        }

        return Task.FromResult(recipes);
    }

    public Task<List<Entree>> ImportEntreesFromExcelAsync(string filePath)
    {
        var entrees = new List<Entree>();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets["Entrees"];

        if (worksheet == null)
        {
            throw new Exception("Entrees worksheet not found in the Excel file.");
        }

        int rowCount = worksheet.Dimension?.Rows ?? 0;

        // Start from row 2 (skip header)
        for (int row = 2; row <= rowCount; row++)
        {
            var name = worksheet.Cells[row, 1].Value?.ToString();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var entree = new Entree
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty,
                MenuPrice = decimal.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out var menuPrice) ? menuPrice : 0,
                EntreeRecipes = new List<EntreeRecipe>(),
                EntreeIngredients = new List<EntreeIngredient>(),
                CreatedAt = DateTime.UtcNow
            };

            entrees.Add(entree);
        }

        return Task.FromResult(entrees);
    }
}
