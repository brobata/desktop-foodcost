using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class CsvExportService : ICsvExportService
{
    public async Task<string> ExportIngredientsToCsvAsync(List<Ingredient> ingredients, string filePath)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Name,Category,Current Price,Unit,Vendor,Vendor SKU,Case Quantity,Use Alternate Unit,Created At,Modified At");

        // Data rows
        foreach (var ingredient in ingredients)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsvField(ingredient.Name),
                EscapeCsvField(ingredient.Category ?? ""),
                ingredient.CurrentPrice.ToString("F2", CultureInfo.InvariantCulture),
                EscapeCsvField(ingredient.Unit.ToString()),
                EscapeCsvField(ingredient.VendorName ?? ""),
                EscapeCsvField(ingredient.VendorSku ?? ""),
                ingredient.CaseQuantity.ToString("F2", CultureInfo.InvariantCulture),
                ingredient.UseAlternateUnit.ToString(),
                ingredient.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                ingredient.ModifiedAt.ToString("yyyy-MM-dd HH:mm:ss")
            ));
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        return filePath;
    }

    public async Task<string> ExportRecipesToCsvAsync(List<Recipe> recipes, string filePath)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Name,Category,Description,Yield,Yield Unit,Prep Time (min),Total Cost,Cost Per Serving,Suggested Menu Price,Difficulty,Tags,Dietary Labels,Created At,Modified At");

        // Data rows
        foreach (var recipe in recipes)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsvField(recipe.Name),
                EscapeCsvField(recipe.Category ?? ""),
                EscapeCsvField(recipe.Description ?? ""),
                recipe.Yield.ToString("F2", CultureInfo.InvariantCulture),
                EscapeCsvField(recipe.YieldUnit),
                recipe.PrepTimeMinutes?.ToString() ?? "",
                recipe.TotalCost.ToString("F2", CultureInfo.InvariantCulture),
                recipe.CostPerServing.ToString("F2", CultureInfo.InvariantCulture),
                recipe.SuggestedMenuPrice?.ToString("F2", CultureInfo.InvariantCulture) ?? "",
                EscapeCsvField(recipe.Difficulty.ToString()),
                EscapeCsvField(recipe.Tags ?? ""),
                EscapeCsvField(recipe.DietaryLabels ?? ""),
                recipe.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                recipe.ModifiedAt.ToString("yyyy-MM-dd HH:mm:ss")
            ));
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        return filePath;
    }

    public async Task<string> ExportEntreesToCsvAsync(List<Entree> entrees, string filePath)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Name,Category,Description,Menu Price,Food Cost,Food Cost %,Contribution Margin,Created At,Modified At");

        // Data rows
        foreach (var entree in entrees)
        {
            var foodCost = entree.TotalCost;
            var foodCostPercent = entree.MenuPrice.HasValue && entree.MenuPrice.Value > 0
                ? (foodCost / entree.MenuPrice.Value) * 100
                : 0;
            var contributionMargin = entree.MenuPrice.HasValue
                ? entree.MenuPrice.Value - foodCost
                : 0;

            csv.AppendLine(string.Join(",",
                EscapeCsvField(entree.Name),
                EscapeCsvField(entree.Category ?? ""),
                EscapeCsvField(entree.Description ?? ""),
                entree.MenuPrice?.ToString("F2", CultureInfo.InvariantCulture) ?? "",
                foodCost.ToString("F2", CultureInfo.InvariantCulture),
                foodCostPercent.ToString("F1", CultureInfo.InvariantCulture),
                contributionMargin.ToString("F2", CultureInfo.InvariantCulture),
                entree.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                entree.ModifiedAt.ToString("yyyy-MM-dd HH:mm:ss")
            ));
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        return filePath;
    }

    public async Task<string> ExportVendorComparisonToCsvAsync(VendorComparisonReport report, string filePath)
    {
        var csv = new StringBuilder();

        // Summary section
        csv.AppendLine("Vendor Comparison Report");
        csv.AppendLine($"Generated Date,{report.GeneratedDate:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Total Ingredients,{report.TotalIngredients}");
        csv.AppendLine($"Total Current Cost,${report.TotalCurrentCost:F2}");
        csv.AppendLine($"Potential Savings,${report.PotentialSavings:F2}");
        csv.AppendLine($"Savings Percent,{report.SavingsPercent:F1}%");
        csv.AppendLine();

        // Vendor summaries
        csv.AppendLine("Vendor Name,Ingredient Count,Total Spend,Average Price,Price Ranking");
        foreach (var vendor in report.VendorSummaries)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsvField(vendor.VendorName),
                vendor.IngredientCount,
                vendor.TotalSpend.ToString("F2", CultureInfo.InvariantCulture),
                vendor.AveragePrice.ToString("F2", CultureInfo.InvariantCulture),
                EscapeCsvField(vendor.PriceRanking)
            ));
        }

        csv.AppendLine();

        // Savings opportunities
        csv.AppendLine("Top Savings Opportunities");
        csv.AppendLine("Ingredient,Current Vendor,Current Price,Recommended Vendor,Recommended Price,Savings Amount,Savings %,Unit");
        foreach (var opportunity in report.TopSavingsOpportunities)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsvField(opportunity.IngredientName),
                EscapeCsvField(opportunity.CurrentVendor),
                opportunity.CurrentPrice.ToString("F2", CultureInfo.InvariantCulture),
                EscapeCsvField(opportunity.RecommendedVendor),
                opportunity.RecommendedPrice.ToString("F2", CultureInfo.InvariantCulture),
                opportunity.SavingsAmount.ToString("F2", CultureInfo.InvariantCulture),
                opportunity.SavingsPercent.ToString("F1", CultureInfo.InvariantCulture),
                EscapeCsvField(opportunity.Unit)
            ));
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        return filePath;
    }

    public async Task<string> ExportProfitabilityReportToCsvAsync(RecipeProfitabilityReport report, string filePath)
    {
        var csv = new StringBuilder();

        // Summary section
        csv.AppendLine("Recipe Profitability Report");
        csv.AppendLine($"Generated Date,{report.GeneratedDate:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Total Recipes,{report.TotalRecipes}");
        csv.AppendLine($"Average Food Cost %,{report.AverageFoodCostPercent:F1}%");
        csv.AppendLine($"Average Contribution Margin,${report.AverageContributionMargin:F2}");
        csv.AppendLine();

        csv.AppendLine($"High Profitability Count,{report.Summary.HighProfitabilityCount}");
        csv.AppendLine($"Moderate Profitability Count,{report.Summary.ModerateProfitabilityCount}");
        csv.AppendLine($"Low Profitability Count,{report.Summary.LowProfitabilityCount}");
        csv.AppendLine($"No Pricing Data Count,{report.Summary.NoPricingDataCount}");
        csv.AppendLine();

        // Recipe details
        csv.AppendLine("Rank,Recipe Name,Category,Total Cost,Cost Per Serving,Suggested Price,Food Cost %,Profit Margin,Contribution Margin,Profitability Level,Recommendation");
        foreach (var recipe in report.Recipes)
        {
            csv.AppendLine(string.Join(",",
                recipe.Rank > 0 ? recipe.Rank.ToString() : "",
                EscapeCsvField(recipe.RecipeName),
                EscapeCsvField(recipe.Category),
                recipe.TotalCost.ToString("F2", CultureInfo.InvariantCulture),
                recipe.CostPerServing.ToString("F2", CultureInfo.InvariantCulture),
                recipe.SuggestedPrice?.ToString("F2", CultureInfo.InvariantCulture) ?? "",
                recipe.FoodCostPercent.ToString("F1", CultureInfo.InvariantCulture),
                recipe.ProfitMargin?.ToString("F1", CultureInfo.InvariantCulture) ?? "",
                recipe.ContributionMargin?.ToString("F2", CultureInfo.InvariantCulture) ?? "",
                EscapeCsvField(recipe.ProfitabilityLevel.ToString()),
                EscapeCsvField(recipe.Recommendation)
            ));
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        return filePath;
    }

    /// <summary>
    /// Escape special characters in CSV fields (quotes, commas, newlines)
    /// </summary>
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        // If field contains comma, quote, or newline, wrap in quotes and escape internal quotes
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}
