using Dfc.Core.Enums;
using Dfc.Core.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IExcelImportService
{
    Task<ImportResult> ImportIngredientsFromExcelAsync(string filePath, Guid locationId);
}

public class ExcelImportService : IExcelImportService
{
    public ExcelImportService()
    {
        // Set EPPlus license (NonCommercial for free use in EPPlus 8+)
        // This must be set before creating any ExcelPackage instances
        ExcelPackage.License.SetNonCommercialPersonal("Desktop Food Cost");
    }

    public async Task<ImportResult> ImportIngredientsFromExcelAsync(string filePath, Guid locationId)
    {
        return await Task.Run(() =>
        {
            var result = new ImportResult();

            try
            {
                if (!File.Exists(filePath))
                {
                    result.Success = false;
                    result.ErrorMessage = "File not found";
                    return result;
                }

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "No worksheets found in Excel file";
                    return result;
                }

                // Find column indices from header row
                var columnMap = new Dictionary<string, int>();
                var headerRow = 1;

                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = worksheet.Cells[headerRow, col].Text?.Trim().ToLowerInvariant();
                    if (!string.IsNullOrEmpty(header))
                    {
                        columnMap[header] = col;
                    }
                }

                // Validate required columns
                if (!columnMap.ContainsKey("name"))
                {
                    result.Success = false;
                    result.ErrorMessage = "Required column 'Name' not found";
                    return result;
                }

                if (!columnMap.ContainsKey("price"))
                {
                    result.Success = false;
                    result.ErrorMessage = "Required column 'Price' not found";
                    return result;
                }

                if (!columnMap.ContainsKey("unit"))
                {
                    result.Success = false;
                    result.ErrorMessage = "Required column 'Unit' not found";
                    return result;
                }

                // Parse ingredients starting from row 2 (after header)
                for (int row = headerRow + 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var name = worksheet.Cells[row, columnMap["name"]].Text?.Trim();

                        // Skip empty rows
                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        var priceText = worksheet.Cells[row, columnMap["price"]].Text?.Trim();
                        var unitText = worksheet.Cells[row, columnMap["unit"]].Text?.Trim();

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(priceText))
                        {
                            result.Errors.Add($"Row {row}: Missing price for '{name}'");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(unitText))
                        {
                            result.Errors.Add($"Row {row}: Missing unit for '{name}'");
                            continue;
                        }

                        if (!decimal.TryParse(priceText, out var price))
                        {
                            result.Errors.Add($"Row {row}: Invalid price '{priceText}' for '{name}'");
                            continue;
                        }

                        if (!TryParseUnit(unitText, out var unit))
                        {
                            result.Errors.Add($"Row {row}: Invalid unit '{unitText}' for '{name}'. Valid units: {GetValidUnitsString()}");
                            continue;
                        }

                        var ingredient = new Ingredient
                        {
                            Id = Guid.NewGuid(),
                            Name = name,
                            CurrentPrice = price,
                            Unit = unit,
                            LocationId = locationId,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        };

                        // Optional fields
                        if (columnMap.ContainsKey("vendor"))
                        {
                            ingredient.VendorName = worksheet.Cells[row, columnMap["vendor"]].Text?.Trim();
                        }

                        if (columnMap.ContainsKey("sku"))
                        {
                            ingredient.VendorSku = worksheet.Cells[row, columnMap["sku"]].Text?.Trim();
                        }

                        if (columnMap.ContainsKey("category"))
                        {
                            ingredient.Category = worksheet.Cells[row, columnMap["category"]].Text?.Trim();
                        }

                        result.Ingredients.Add(ingredient);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {row}: {ex.Message}");
                    }
                }

                result.Success = result.Ingredients.Any();
                if (!result.Success && result.Errors.Count == 0)
                {
                    result.ErrorMessage = "No valid ingredients found in file";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error reading Excel file: {ex.Message}";
            }

            return result;
        });
    }

    private bool TryParseUnit(string unitText, out UnitType unit)
    {
        unit = UnitType.Ounce;

        if (string.IsNullOrWhiteSpace(unitText))
            return false;

        var normalized = unitText.Trim().ToLowerInvariant()
            .Replace(" ", "")
            .Replace("-", "");

        // Try exact enum parse first
        if (Enum.TryParse<UnitType>(unitText, ignoreCase: true, out unit))
            return true;

        // Try common abbreviations and variations
        unit = normalized switch
        {
            "tsp" or "teaspoon" or "teaspoons" => UnitType.Teaspoon,
            "tbsp" or "tablespoon" or "tablespoons" => UnitType.Tablespoon,
            "floz" or "fl.oz" or "fluidounce" or "fluidounces" => UnitType.FluidOunce,
            "cup" or "cups" or "c" => UnitType.Cup,
            "pint" or "pints" or "pt" => UnitType.Pint,
            "quart" or "quarts" or "qt" => UnitType.Quart,
            "gallon" or "gallons" or "gal" => UnitType.Gallon,
            "oz" or "ounce" or "ounces" => UnitType.Ounce,
            "lb" or "lbs" or "pound" or "pounds" or "#" => UnitType.Pound,
            "ml" or "milliliter" or "milliliters" => UnitType.Milliliter,
            "l" or "liter" or "liters" => UnitType.Liter,
            "g" or "gram" or "grams" => UnitType.Gram,
            "kg" or "kilogram" or "kilograms" => UnitType.Kilogram,
            "ea" or "each" or "piece" or "pieces" => UnitType.Each,
            "doz" or "dozen" or "dozens" => UnitType.Dozen,
            _ => UnitType.Ounce // Default fallback
        };

        return normalized switch
        {
            "tsp" or "teaspoon" or "teaspoons" or
            "tbsp" or "tablespoon" or "tablespoons" or
            "floz" or "fl.oz" or "fluidounce" or "fluidounces" or
            "cup" or "cups" or "c" or
            "pint" or "pints" or "pt" or
            "quart" or "quarts" or "qt" or
            "gallon" or "gallons" or "gal" or
            "oz" or "ounce" or "ounces" or
            "lb" or "lbs" or "pound" or "pounds" or "#" or
            "ml" or "milliliter" or "milliliters" or
            "l" or "liter" or "liters" or
            "g" or "gram" or "grams" or
            "kg" or "kilogram" or "kilograms" or
            "ea" or "each" or "piece" or "pieces" or
            "doz" or "dozen" or "dozens" => true,
            _ => false
        };
    }

    private string GetValidUnitsString()
    {
        return "oz, lb, cup, pint, quart, gallon, tsp, tbsp, fl oz, ml, l, g, kg, each, dozen";
    }
}

public class ImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Ingredient> Ingredients { get; set; } = new();
    public int SuccessCount { get; set; }
}
