using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface ICsvExportService
{
    Task<string> ExportIngredientsToCsvAsync(List<Ingredient> ingredients, string filePath);
    Task<string> ExportRecipesToCsvAsync(List<Recipe> recipes, string filePath);
    Task<string> ExportEntreesToCsvAsync(List<Entree> entrees, string filePath);
    Task<string> ExportVendorComparisonToCsvAsync(VendorComparisonReport report, string filePath);
    Task<string> ExportProfitabilityReportToCsvAsync(RecipeProfitabilityReport report, string filePath);
}
