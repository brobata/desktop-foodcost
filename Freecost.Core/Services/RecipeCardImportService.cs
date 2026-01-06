using Freecost.Core.Enums;
using Freecost.Core.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IRecipeCardImportService
{
    Task<RecipeCardImportResult> ParseRecipeCardsAsync(string filePath, Guid locationId);
}

public class RecipeCardImportService : IRecipeCardImportService
{
    private readonly IIngredientMatchingService _matchingService;

    public RecipeCardImportService(IIngredientMatchingService matchingService)
    {
        _matchingService = matchingService;
        // Note: EPPlus license is configured globally in App.axaml.cs
    }

    public async Task<RecipeCardImportResult> ParseRecipeCardsAsync(string filePath, Guid locationId)
    {
        var result = new RecipeCardImportResult();

        try
        {
            Console.WriteLine($"[RECIPE IMPORT] Starting import from: {filePath}");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT] Starting import from: {filePath}");

            if (!File.Exists(filePath))
            {
                result.Success = false;
                result.ErrorMessage = "File not found";
                Console.WriteLine($"[RECIPE IMPORT] ERROR: File not found");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT] ERROR: File not found");
                return result;
            }

            Console.WriteLine($"[RECIPE IMPORT] Opening Excel file...");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT] Opening Excel file...");
            using var package = new ExcelPackage(new FileInfo(filePath));

            if (package.Workbook.Worksheets.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No worksheets found in Excel file";
                Console.WriteLine($"[RECIPE IMPORT] ERROR: No worksheets found");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT] ERROR: No worksheets found");
                return result;
            }

            result.TotalTabs = package.Workbook.Worksheets.Count;
            Console.WriteLine($"[RECIPE IMPORT] Found {result.TotalTabs} worksheet(s)");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT] Found {result.TotalTabs} worksheet(s)");

            // Parse each tab as a recipe card
            for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
            {
                var worksheet = package.Workbook.Worksheets[i];
                Console.WriteLine($"\n[RECIPE IMPORT] ========================================");
                Console.WriteLine($"[RECIPE IMPORT] Processing worksheet {i + 1}/{package.Workbook.Worksheets.Count}: '{worksheet.Name}'");
                System.Diagnostics.Debug.WriteLine($"\n[RECIPE IMPORT] ========================================");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT] Processing worksheet {i + 1}/{package.Workbook.Worksheets.Count}: '{worksheet.Name}'");

                try
                {
                    var preview = await ParseRecipeCardTab(worksheet, i, locationId);
                    result.RecipePreviews.Add(preview);

                    if (preview.IsValid)
                    {
                        result.ValidRecipes++;
                        Console.WriteLine($"[RECIPE IMPORT] ✓ Valid recipe: '{preview.Name}' ({preview.Ingredients.Count} ingredients)");
                    }
                    else
                    {
                        result.InvalidRecipes++;
                        result.Errors.AddRange(preview.ValidationErrors.Select(e => $"{worksheet.Name}: {e}"));
                        Console.WriteLine($"[RECIPE IMPORT] ✗ Invalid recipe: '{preview.Name}'");
                        Console.WriteLine($"[RECIPE IMPORT]   Validation errors:");
                        foreach (var error in preview.ValidationErrors)
                        {
                            Console.WriteLine($"[RECIPE IMPORT]     - {error}");
                        }
                    }

                    // Collect unmatched ingredients
                    foreach (var ingredient in preview.Ingredients.Where(i => !i.IsMatched))
                    {
                        var unmatched = result.UnmatchedIngredients.FirstOrDefault(u =>
                            u.Name.Equals(ingredient.IngredientName, StringComparison.OrdinalIgnoreCase));

                        if (unmatched == null)
                        {
                            var suggestions = await _matchingService.GetSuggestionsAsync(ingredient.IngredientName, locationId);
                            unmatched = new UnmatchedIngredient
                            {
                                Name = ingredient.IngredientName,
                                Suggestions = new System.Collections.ObjectModel.ObservableCollection<IngredientMatchSuggestion>(suggestions)
                            };
                            result.UnmatchedIngredients.Add(unmatched);
                        }

                        unmatched.AppearsInRecipes.Add(preview.Name);
                        unmatched.UsageCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Tab '{worksheet.Name}': {ex.Message}");
                    result.InvalidRecipes++;
                    Console.WriteLine($"[RECIPE IMPORT] ✗ EXCEPTION parsing worksheet '{worksheet.Name}': {ex.Message}");
                    Console.WriteLine($"[RECIPE IMPORT]   Stack trace: {ex.StackTrace}");
                    System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT] ✗ EXCEPTION parsing worksheet '{worksheet.Name}': {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]   Stack trace: {ex.StackTrace}");
                }
            }

            result.Success = result.ValidRecipes > 0;

            if (!result.Success && result.Errors.Count == 0)
            {
                result.ErrorMessage = "No valid recipes found in file";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error reading Excel file: {ex.Message}";
            Console.WriteLine($"[RECIPE IMPORT] ✗ FATAL ERROR: {ex.Message}");
            Console.WriteLine($"[RECIPE IMPORT]   Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT] ✗ FATAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]   Stack trace: {ex.StackTrace}");
        }

        return result;
    }

    private async Task<RecipeCardPreview> ParseRecipeCardTab(ExcelWorksheet worksheet, int tabIndex, Guid locationId)
    {
        Console.WriteLine($"[RECIPE IMPORT]   Parsing recipe card tab...");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]   Parsing recipe card tab: '{worksheet.Name}'");

        var preview = new RecipeCardPreview
        {
            TabName = worksheet.Name,
            TabIndex = tabIndex
        };

        // Try to extract recipe metadata and ingredients from the worksheet
        // This uses a flexible approach to handle different card formats

        // Strategy: Scan the worksheet for labeled fields and ingredient tables
        var data = ExtractWorksheetData(worksheet);
        Console.WriteLine($"[RECIPE IMPORT]   Extracted {data.Count} non-empty cells from worksheet");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]   Extracted {data.Count} non-empty cells from worksheet");

        // Extract recipe name (required)
        preview.Name = FindLabeledValue(data, "recipe name", "name", "title", "dish") ?? worksheet.Name;
        Console.WriteLine($"[RECIPE IMPORT]   Recipe name: '{preview.Name}'");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]   Recipe name: '{preview.Name}'");

        if (string.IsNullOrWhiteSpace(preview.Name) || preview.Name == worksheet.Name)
        {
            // Try first non-empty cell as name
            preview.Name = data.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.Value))?.Value ?? worksheet.Name;
        }

        // Extract other metadata
        preview.Description = FindLabeledValue(data, "description", "desc", "summary");
        preview.Category = FindLabeledValue(data, "category", "type", "classification");

        // Extract instructions - try multi-row procedure first, then fallback to single value
        preview.Instructions = ExtractProcedureSteps(data) ?? FindLabeledValue(data, "instructions", "directions", "method", "preparation", "procedure");
        Console.WriteLine($"[RECIPE IMPORT]   Instructions: {(string.IsNullOrWhiteSpace(preview.Instructions) ? "None" : $"{preview.Instructions.Length} characters")}");

        preview.Notes = FindLabeledValue(data, "notes", "note", "comments", "remarks");
        preview.Tags = FindLabeledValue(data, "tags", "keywords");

        // Extract yield
        var yieldText = FindLabeledValue(data, "yield", "servings", "serves", "portions", "makes");
        if (!string.IsNullOrWhiteSpace(yieldText))
        {
            ParseYield(yieldText, out decimal yieldAmount, out string yieldUnit);
            preview.Yield = yieldAmount;
            preview.YieldUnit = yieldUnit;
            Console.WriteLine($"[RECIPE IMPORT]   Yield: {yieldAmount} {yieldUnit} (from text: '{yieldText}')");
        }
        else
        {
            // Default yield if not found
            preview.Yield = 1;
            preview.YieldUnit = "serving";
            Console.WriteLine($"[RECIPE IMPORT]   Yield: Using default (1 serving)");
        }

        // Extract prep time
        var prepTimeText = FindLabeledValue(data, "prep time", "preparation time", "prep", "cook time", "time");
        if (!string.IsNullOrWhiteSpace(prepTimeText))
        {
            preview.PrepTimeMinutes = ParseTimeToMinutes(prepTimeText);
            Console.WriteLine($"[RECIPE IMPORT]   Prep time: {preview.PrepTimeMinutes} minutes (from text: '{prepTimeText}')");
        }

        // Extract ingredients (most complex part)
        Console.WriteLine($"[RECIPE IMPORT]   Extracting ingredients...");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]   Extracting ingredients...");
        preview.Ingredients = await ExtractIngredientsFromWorksheet(worksheet, data, locationId);
        Console.WriteLine($"[RECIPE IMPORT]   Extracted {preview.Ingredients.Count} ingredient(s)");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]   Extracted {preview.Ingredients.Count} ingredient(s)");

        // Validation
        Console.WriteLine($"[RECIPE IMPORT]   Validating recipe...");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]   Validating recipe...");
        ValidateRecipePreview(preview);

        return preview;
    }

    private List<CellData> ExtractWorksheetData(ExcelWorksheet worksheet)
    {
        var data = new List<CellData>();

        if (worksheet.Dimension == null)
            return data;

        for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
        {
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[row, col];
                var value = cell.Text?.Trim();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    data.Add(new CellData
                    {
                        Row = row,
                        Column = col,
                        Value = value,
                        IsBold = cell.Style.Font.Bold
                    });
                }
            }
        }

        return data;
    }

    private string? ExtractProcedureSteps(List<CellData> data)
    {
        // Find "Procedure" or similar header
        var procedureHeader = data.FirstOrDefault(d =>
            d.Value.Equals("Procedure", StringComparison.OrdinalIgnoreCase) ||
            d.Value.Equals("Instructions", StringComparison.OrdinalIgnoreCase) ||
            d.Value.Equals("Directions", StringComparison.OrdinalIgnoreCase) ||
            d.Value.Equals("Method", StringComparison.OrdinalIgnoreCase) ||
            d.Value.Equals("Steps", StringComparison.OrdinalIgnoreCase));

        if (procedureHeader == null)
            return null;

        Console.WriteLine($"[RECIPE IMPORT]   Found procedure header at row {procedureHeader.Row}");

        // Get all cells in column 2 (the description column) below the procedure header
        var procedureSteps = data
            .Where(d => d.Row > procedureHeader.Row && d.Column >= 2)  // Column 2 or higher
            .OrderBy(d => d.Row)
            .ThenBy(d => d.Column)
            .ToList();

        if (!procedureSteps.Any())
            return null;

        // Group by row and take the rightmost non-number column (the description, not the step number)
        var steps = new List<string>();
        foreach (var rowGroup in procedureSteps.GroupBy(c => c.Row))
        {
            // Get the cell with the longest text (usually the description, not the step number)
            var descriptionCell = rowGroup.OrderByDescending(c => c.Value.Length).First();

            // Skip if it's just a number or empty
            if (string.IsNullOrWhiteSpace(descriptionCell.Value) || int.TryParse(descriptionCell.Value, out _))
                continue;

            // Stop if we hit 3 consecutive empty rows or another section header
            if (descriptionCell.IsBold && descriptionCell.Value.EndsWith(":"))
                break;

            steps.Add(descriptionCell.Value);

            // Stop after finding reasonable number of empty rows
            if (steps.Count > 0 && rowGroup.All(c => string.IsNullOrWhiteSpace(c.Value)))
            {
                var emptyCount = procedureSteps
                    .Where(c => c.Row > descriptionCell.Row && c.Row <= descriptionCell.Row + 3)
                    .GroupBy(c => c.Row)
                    .Count(g => g.All(c => string.IsNullOrWhiteSpace(c.Value)));

                if (emptyCount >= 3)
                    break;
            }
        }

        if (!steps.Any())
            return null;

        // Join steps with line breaks
        var result = string.Join("\n", steps.Select((s, i) => $"{i + 1}. {s}"));
        Console.WriteLine($"[RECIPE IMPORT]   Extracted {steps.Count} procedure step(s)");
        return result;
    }

    private string? FindLabeledValue(List<CellData> data, params string[] labelVariations)
    {
        foreach (var label in labelVariations)
        {
            // Look for "Label:" pattern in same cell or adjacent cell
            var labelCell = data.FirstOrDefault(d =>
                d.Value.Contains(label, StringComparison.OrdinalIgnoreCase) &&
                (d.Value.Contains(":") || d.Value.Contains("=") || d.IsBold));

            if (labelCell != null)
            {
                // Check if value is in same cell after colon/equals
                var colonIndex = labelCell.Value.IndexOfAny(new[] { ':', '=' });
                if (colonIndex >= 0 && colonIndex < labelCell.Value.Length - 1)
                {
                    var value = labelCell.Value.Substring(colonIndex + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }

                // Check adjacent cells (right or below)
                var adjacentValue = data.FirstOrDefault(d =>
                    (d.Row == labelCell.Row && d.Column == labelCell.Column + 1) ||
                    (d.Row == labelCell.Row + 1 && d.Column == labelCell.Column))?.Value;

                if (!string.IsNullOrWhiteSpace(adjacentValue))
                    return adjacentValue;
            }
        }

        return null;
    }

    private async Task<List<RecipeIngredientPreview>> ExtractIngredientsFromWorksheet(
        ExcelWorksheet worksheet, List<CellData> data, Guid locationId)
    {
        var ingredients = new List<RecipeIngredientPreview>();

        // Strategy 1: Look for ingredient table (columns: Quantity, Unit, Ingredient)
        Console.WriteLine($"[RECIPE IMPORT]     Strategy 1: Looking for ingredient table...");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]     Strategy 1: Looking for ingredient table...");
        var tableIngredients = await ExtractIngredientsFromTable(worksheet, data, locationId);
        if (tableIngredients.Any())
        {
            Console.WriteLine($"[RECIPE IMPORT]     ✓ Found {tableIngredients.Count} ingredients using table strategy");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]     ✓ Found {tableIngredients.Count} ingredients using table strategy");
            return tableIngredients;
        }
        else
        {
            Console.WriteLine($"[RECIPE IMPORT]     ✗ No ingredients found using table strategy");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]     ✗ No ingredients found using table strategy");
        }

        // Strategy 2: Look for ingredient list (text-based, each line is an ingredient)
        Console.WriteLine($"[RECIPE IMPORT]     Strategy 2: Looking for ingredient list...");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]     Strategy 2: Looking for ingredient list...");
        var listIngredients = await ExtractIngredientsFromList(worksheet, data, locationId);
        if (listIngredients.Any())
        {
            Console.WriteLine($"[RECIPE IMPORT]     ✓ Found {listIngredients.Count} ingredients using list strategy");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]     ✓ Found {listIngredients.Count} ingredients using list strategy");
            return listIngredients;
        }
        else
        {
            Console.WriteLine($"[RECIPE IMPORT]     ✗ No ingredients found using list strategy");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]     ✗ No ingredients found using list strategy");
        }

        Console.WriteLine($"[RECIPE IMPORT]     WARNING: No ingredients extracted by any strategy!");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]     WARNING: No ingredients extracted by any strategy!");
        return ingredients;
    }

    private async Task<List<RecipeIngredientPreview>> ExtractIngredientsFromTable(
        ExcelWorksheet worksheet, List<CellData> data, Guid locationId)
    {
        var ingredients = new List<RecipeIngredientPreview>();

        // Find header row with "Quantity", "Unit", "Ingredient" (or variations)
        var headerRow = FindIngredientTableHeader(data);
        if (headerRow == null)
        {
            Console.WriteLine($"[RECIPE IMPORT]       No table header found, cannot extract ingredients");
            return ingredients;
        }

        // Parse rows below header
        var startRow = headerRow.Row + 1;

        // Skip first row after header if needed (e.g., batch multiplier row in multi-batch format)
        if (headerRow.SkipFirstRow)
        {
            Console.WriteLine($"[RECIPE IMPORT]       Skipping row {startRow} (batch multiplier row)");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       Skipping row {startRow} (batch multiplier row)");
            startRow++;
        }

        var endRow = worksheet.Dimension?.End.Row ?? startRow;
        Console.WriteLine($"[RECIPE IMPORT]       Parsing ingredient rows {startRow} to {endRow}");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       Parsing ingredient rows {startRow} to {endRow}");

        // DEBUG: Show what cells have data in the first 3 rows after header
        for (int debugRow = startRow; debugRow <= Math.Min(startRow + 2, endRow); debugRow++)
        {
            var debugCells = data.Where(d => d.Row == debugRow).ToList();
            if (debugCells.Any())
            {
                var debugInfo = string.Join(", ", debugCells.Select(c => $"Col{c.Column}='{c.Value}'"));
                Console.WriteLine($"[RECIPE IMPORT]       DEBUG Row {debugRow} cells: {debugInfo}");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       DEBUG Row {debugRow} cells: {debugInfo}");
            }
            else
            {
                Console.WriteLine($"[RECIPE IMPORT]       DEBUG Row {debugRow}: NO DATA");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       DEBUG Row {debugRow}: NO DATA");
            }
        }

        int consecutiveEmptyRows = 0;
        for (int row = startRow; row <= endRow; row++)
        {
            string? quantityText;
            string? unitText;
            var ingredientText = worksheet.Cells[row, headerRow.IngredientCol].Text?.Trim();

            // Skip empty rows but track how many consecutive empties we've seen
            if (string.IsNullOrWhiteSpace(ingredientText))
            {
                consecutiveEmptyRows++;
                Console.WriteLine($"[RECIPE IMPORT]       Row {row}: Empty ingredient name, skipping (consecutive empties: {consecutiveEmptyRows})");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       Row {row}: Empty ingredient name, skipping (consecutive empties: {consecutiveEmptyRows})");

                // Stop if we hit 3 consecutive empty rows (likely end of ingredient table)
                if (consecutiveEmptyRows >= 3)
                {
                    Console.WriteLine($"[RECIPE IMPORT]       Stopping: Found {consecutiveEmptyRows} consecutive empty rows");
                    System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       Stopping: Found {consecutiveEmptyRows} consecutive empty rows");
                    break;
                }

                continue;
            }

            // Reset counter when we find data
            consecutiveEmptyRows = 0;

            Console.WriteLine($"[RECIPE IMPORT]       Row {row}: Ingredient='{ingredientText}'");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       Row {row}: Ingredient='{ingredientText}'");

            decimal quantity;
            UnitType unit;

            if (headerRow.IsCombinedQuantityUnit)
            {
                // Multi-batch format: Parse combined "quantity unit" (e.g., "2 Qt")
                var combinedText = worksheet.Cells[row, headerRow.QuantityCol].Text?.Trim();
                Console.WriteLine($"[RECIPE IMPORT]         Combined text from col {headerRow.QuantityCol}: '{combinedText}'");

                if (string.IsNullOrWhiteSpace(combinedText))
                {
                    quantity = 1;
                    unit = UnitType.Each;
                    quantityText = "1";
                    unitText = "ea";
                    Console.WriteLine($"[RECIPE IMPORT]         Empty combined text, using defaults: 1 ea");
                }
                else
                {
                    // Split on whitespace: "2 Qt" -> ["2", "Qt"]
                    var parts = combinedText.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine($"[RECIPE IMPORT]         Split into {parts.Length} part(s): [{string.Join(", ", parts)}]");

                    if (parts.Length >= 2)
                    {
                        quantityText = parts[0];
                        unitText = parts[1];
                    }
                    else if (parts.Length == 1)
                    {
                        // Only quantity provided, assume "Each"
                        quantityText = parts[0];
                        unitText = "ea";
                    }
                    else
                    {
                        quantityText = "1";
                        unitText = "ea";
                    }

                    // Parse quantity
                    if (!TryParseQuantity(quantityText, out quantity))
                    {
                        Console.WriteLine($"[RECIPE IMPORT]         Failed to parse quantity '{quantityText}', using default: 1");
                        quantity = 1;
                    }
                    else
                    {
                        Console.WriteLine($"[RECIPE IMPORT]         Parsed quantity: {quantity}");
                    }

                    // Parse unit
                    if (!TryParseUnit(unitText, out unit))
                    {
                        Console.WriteLine($"[RECIPE IMPORT]         Failed to parse unit '{unitText}', using default: Each");
                        unit = UnitType.Each;
                    }
                    else
                    {
                        Console.WriteLine($"[RECIPE IMPORT]         Parsed unit: {unit}");
                    }
                }
            }
            else
            {
                // Traditional format: separate quantity and unit columns
                quantityText = worksheet.Cells[row, headerRow.QuantityCol].Text?.Trim();
                unitText = worksheet.Cells[row, headerRow.UnitCol].Text?.Trim();
                Console.WriteLine($"[RECIPE IMPORT]         Quantity from col {headerRow.QuantityCol}: '{quantityText}'");
                Console.WriteLine($"[RECIPE IMPORT]         Unit from col {headerRow.UnitCol}: '{unitText}'");

                // Parse quantity
                if (!TryParseQuantity(quantityText, out quantity))
                {
                    Console.WriteLine($"[RECIPE IMPORT]         Failed to parse quantity '{quantityText}', using default: 1");
                    quantity = 1; // Default to 1 if can't parse
                }
                else
                {
                    Console.WriteLine($"[RECIPE IMPORT]         Parsed quantity: {quantity}");
                }

                // Parse unit
                if (!TryParseUnit(unitText, out unit))
                {
                    Console.WriteLine($"[RECIPE IMPORT]         Failed to parse unit '{unitText}', using default: Each");
                    unit = UnitType.Each; // Default
                }
                else
                {
                    Console.WriteLine($"[RECIPE IMPORT]         Parsed unit: {unit}");
                }
            }

            // Match ingredient
            Console.WriteLine($"[RECIPE IMPORT]         Matching ingredient '{ingredientText}' to database...");
            var matchResult = await _matchingService.FindBestMatchAsync(ingredientText, locationId);

            if (matchResult.IsMatched)
            {
                Console.WriteLine($"[RECIPE IMPORT]         ✓ Matched to '{matchResult.IngredientName}' (confidence: {matchResult.Confidence}%, method: {matchResult.MatchMethod})");
            }
            else
            {
                Console.WriteLine($"[RECIPE IMPORT]         ✗ No match found for '{ingredientText}'");
            }

            var preview = new RecipeIngredientPreview
            {
                OriginalText = $"{quantityText} {unitText} {ingredientText}".Trim(),
                IngredientName = ingredientText,
                Quantity = quantity,
                Unit = unit,
                IsMatched = matchResult.IsMatched,
                MatchedIngredientId = matchResult.IsMatched ? matchResult.IngredientId : null,
                MatchedIngredientName = matchResult.IngredientName,
                MatchConfidence = matchResult.Confidence,
                MatchMethod = matchResult.MatchMethod
            };

            ingredients.Add(preview);
        }

        Console.WriteLine($"[RECIPE IMPORT]       ✓ Extracted {ingredients.Count} ingredient(s) from table");
        return ingredients;
    }

    private async Task<List<RecipeIngredientPreview>> ExtractIngredientsFromList(
        ExcelWorksheet worksheet, List<CellData> data, Guid locationId)
    {
        var ingredients = new List<RecipeIngredientPreview>();

        // Find "Ingredients" section header
        var ingredientsHeader = data.FirstOrDefault(d =>
            d.Value.Equals("Ingredients", StringComparison.OrdinalIgnoreCase) ||
            d.Value.Equals("Components", StringComparison.OrdinalIgnoreCase));

        if (ingredientsHeader == null)
            return ingredients;

        // Get all rows below the header in the same column
        var ingredientRows = data
            .Where(d => d.Row > ingredientsHeader.Row && d.Column == ingredientsHeader.Column)
            .OrderBy(d => d.Row)
            .ToList();

        foreach (var cellData in ingredientRows)
        {
            // Parse ingredient line: "2 cups chicken broth"
            var line = cellData.Value;

            // Stop if we hit another section header
            if (line.EndsWith(":") || cellData.IsBold)
                break;

            var parsed = ParseIngredientLine(line);
            if (parsed != null)
            {
                var matchResult = await _matchingService.FindBestMatchAsync(parsed.IngredientName, locationId);

                var preview = new RecipeIngredientPreview
                {
                    OriginalText = line,
                    IngredientName = parsed.IngredientName,
                    Quantity = parsed.Quantity,
                    Unit = parsed.Unit,
                    IsMatched = matchResult.IsMatched,
                    MatchedIngredientId = matchResult.IsMatched ? matchResult.IngredientId : null,
                    MatchedIngredientName = matchResult.IngredientName,
                    MatchConfidence = matchResult.Confidence,
                    MatchMethod = matchResult.MatchMethod
                };

                ingredients.Add(preview);
            }
        }

        return ingredients;
    }

    private IngredientTableHeader? FindIngredientTableHeader(List<CellData> data)
    {
        Console.WriteLine($"[RECIPE IMPORT]       Searching for ingredient table header...");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       Searching for ingredient table header...");

        // Look for header row containing quantity/unit/ingredient columns
        var groupedByRow = data.GroupBy(d => d.Row);

        foreach (var rowGroup in groupedByRow)
        {
            var cells = rowGroup.ToList();
            Console.WriteLine($"[RECIPE IMPORT]       Checking row {rowGroup.Key}: {string.Join(", ", cells.Select(c => $"Col{c.Column}='{c.Value}'"))}");
            System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       Checking row {rowGroup.Key}: {string.Join(", ", cells.Select(c => $"Col{c.Column}='{c.Value}'"))}");

            // Strategy 1: Look for multi-batch format (INGREDIENT | Batch 1 | Batch 2 | ...)
            int? ingredientHeaderCol = cells.FirstOrDefault(c =>
                c.Value.Contains("ingredient", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Equals("item", StringComparison.OrdinalIgnoreCase))?.Column;

            int? batchCol = cells.FirstOrDefault(c =>
                c.Value.Contains("batch", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("qty", StringComparison.OrdinalIgnoreCase))?.Column;

            if (ingredientHeaderCol.HasValue && batchCol.HasValue)
            {
                // Multi-batch format: Check if quantity and unit are in separate columns or combined
                // Look at the row below header to determine the format
                var dataRow = data.FirstOrDefault(d => d.Row == rowGroup.Key + 2 && d.Column == batchCol.Value);
                var nextCol = data.FirstOrDefault(d => d.Row == rowGroup.Key + 2 && d.Column == batchCol.Value + 1);

                // If the next column contains unit text (like "qt", "ea", etc.), then it's separate columns
                bool isSeparateColumns = nextCol != null && IsLikelyUnitText(nextCol.Value);

                Console.WriteLine($"[RECIPE IMPORT]       ✓ Found MULTI-BATCH format header at row {rowGroup.Key}");
                Console.WriteLine($"[RECIPE IMPORT]         Ingredient column: {ingredientHeaderCol.Value}");
                Console.WriteLine($"[RECIPE IMPORT]         Batch column: {batchCol.Value}");
                Console.WriteLine($"[RECIPE IMPORT]         Format: {(isSeparateColumns ? "Separate qty/unit columns" : "Combined qty+unit")}");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       ✓ Found MULTI-BATCH format header at row {rowGroup.Key}");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]         Ingredient column: {ingredientHeaderCol.Value}");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]         Batch column: {batchCol.Value}");
                System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]         Format: {(isSeparateColumns ? "Separate qty/unit columns" : "Combined qty+unit")}");

                return new IngredientTableHeader
                {
                    Row = rowGroup.Key,
                    IngredientCol = ingredientHeaderCol.Value,
                    QuantityCol = batchCol.Value,
                    UnitCol = isSeparateColumns ? batchCol.Value + 1 : -1,
                    IsCombinedQuantityUnit = !isSeparateColumns,
                    SkipFirstRow = true // Skip batch multiplier row
                };
            }

            // Strategy 2: Look for traditional format (Quantity | Unit | Ingredient)
            int? quantityCol = cells.FirstOrDefault(c =>
                c.Value.Contains("quantity", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("qty", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("amount", StringComparison.OrdinalIgnoreCase))?.Column;

            int? unitCol = cells.FirstOrDefault(c =>
                c.Value.Contains("unit", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("uom", StringComparison.OrdinalIgnoreCase))?.Column;

            int? ingredientCol = cells.FirstOrDefault(c =>
                c.Value.Contains("ingredient", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("item", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("name", StringComparison.OrdinalIgnoreCase))?.Column;

            if (quantityCol.HasValue && unitCol.HasValue && ingredientCol.HasValue)
            {
                Console.WriteLine($"[RECIPE IMPORT]       ✓ Found TRADITIONAL format header at row {rowGroup.Key}");
                Console.WriteLine($"[RECIPE IMPORT]         Quantity column: {quantityCol.Value}");
                Console.WriteLine($"[RECIPE IMPORT]         Unit column: {unitCol.Value}");
                Console.WriteLine($"[RECIPE IMPORT]         Ingredient column: {ingredientCol.Value}");
                return new IngredientTableHeader
                {
                    Row = rowGroup.Key,
                    QuantityCol = quantityCol.Value,
                    UnitCol = unitCol.Value,
                    IngredientCol = ingredientCol.Value,
                    IsCombinedQuantityUnit = false
                };
            }
        }

        Console.WriteLine($"[RECIPE IMPORT]       ✗ No ingredient table header found!");
        System.Diagnostics.Debug.WriteLine($"[RECIPE IMPORT]       ✗ No ingredient table header found!");
        return null;
    }

    private ParsedIngredient? ParseIngredientLine(string line)
    {
        // Pattern: "2 cups chicken broth" or "1/2 tsp salt"
        var pattern = @"^([0-9./\s]+)\s*([a-zA-Z]+)\s+(.+)$";
        var match = Regex.Match(line, pattern);

        if (match.Success)
        {
            var quantityText = match.Groups[1].Value.Trim();
            var unitText = match.Groups[2].Value.Trim();
            var ingredientName = match.Groups[3].Value.Trim();

            if (TryParseQuantity(quantityText, out decimal quantity) &&
                TryParseUnit(unitText, out UnitType unit))
            {
                return new ParsedIngredient
                {
                    Quantity = quantity,
                    Unit = unit,
                    IngredientName = ingredientName
                };
            }
        }

        // Fallback: just ingredient name
        return new ParsedIngredient
        {
            Quantity = 1,
            Unit = UnitType.Each,
            IngredientName = line
        };
    }

    private bool TryParseQuantity(string? text, out decimal quantity)
    {
        quantity = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Handle fractions: "1/2", "1 1/2"
        text = text.Trim();

        // Check for fraction
        if (text.Contains("/"))
        {
            var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            decimal result = 0;

            foreach (var part in parts)
            {
                if (part.Contains("/"))
                {
                    var fracParts = part.Split('/');
                    if (fracParts.Length == 2 &&
                        decimal.TryParse(fracParts[0], out decimal numerator) &&
                        decimal.TryParse(fracParts[1], out decimal denominator) &&
                        denominator != 0)
                    {
                        result += numerator / denominator;
                    }
                }
                else if (decimal.TryParse(part, out decimal whole))
                {
                    result += whole;
                }
            }

            quantity = result;
            return result > 0;
        }

        return decimal.TryParse(text, out quantity);
    }

    private bool IsLikelyUnitText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = text.Trim().ToLowerInvariant();

        // Common unit abbreviations and names
        var unitKeywords = new[]
        {
            "tsp", "teaspoon", "tbsp", "tablespoon", "cup", "oz", "ounce",
            "lb", "pound", "qt", "quart", "gal", "gallon", "pt", "pint",
            "ml", "liter", "gram", "kg", "ea", "each", "doz", "dozen",
            "floz", "fl.oz", "c", "g", "l"
        };

        return unitKeywords.Any(keyword => normalized.Contains(keyword));
    }

    private bool TryParseUnit(string? unitText, out UnitType unit)
    {
        unit = UnitType.Ounce;

        if (string.IsNullOrWhiteSpace(unitText))
            return false;

        var normalized = unitText.Trim().ToLowerInvariant().Replace(" ", "").Replace("-", "");

        // Try exact enum parse first
        if (Enum.TryParse<UnitType>(unitText, ignoreCase: true, out unit))
            return true;

        // Try common abbreviations
        unit = normalized switch
        {
            "tsp" or "teaspoon" or "teaspoons" => UnitType.Teaspoon,
            "tbsp" or "tablespoon" or "tablespoons" or "tbs" => UnitType.Tablespoon,
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
            "ea" or "each" or "piece" or "pieces" or "pc" => UnitType.Each,
            "doz" or "dozen" or "dozens" => UnitType.Dozen,
            _ => UnitType.Each
        };

        return true;
    }

    private void ParseYield(string yieldText, out decimal yieldAmount, out string yieldUnit)
    {
        yieldAmount = 0;
        yieldUnit = "servings";

        // Pattern: "8 servings", "12 portions", "2 dozen"
        var pattern = @"^([0-9./\s]+)\s*(.*)$";
        var match = Regex.Match(yieldText, pattern);

        if (match.Success)
        {
            var amountText = match.Groups[1].Value.Trim();
            var unitText = match.Groups[2].Value.Trim();

            TryParseQuantity(amountText, out yieldAmount);
            if (!string.IsNullOrWhiteSpace(unitText))
            {
                yieldUnit = unitText;
            }
        }
        else
        {
            // Just a number
            if (TryParseQuantity(yieldText, out yieldAmount))
            {
                yieldUnit = "servings";
            }
        }
    }

    private int? ParseTimeToMinutes(string timeText)
    {
        // Pattern: "30 minutes", "1 hour", "1 hr 30 min"
        var totalMinutes = 0;

        var hourMatch = Regex.Match(timeText, @"(\d+)\s*(hour|hr|hours|hrs)", RegexOptions.IgnoreCase);
        if (hourMatch.Success && int.TryParse(hourMatch.Groups[1].Value, out int hours))
        {
            totalMinutes += hours * 60;
        }

        var minuteMatch = Regex.Match(timeText, @"(\d+)\s*(minute|min|minutes|mins)", RegexOptions.IgnoreCase);
        if (minuteMatch.Success && int.TryParse(minuteMatch.Groups[1].Value, out int minutes))
        {
            totalMinutes += minutes;
        }

        // If no match, try parsing as plain number (assume minutes)
        if (totalMinutes == 0 && int.TryParse(timeText, out int plainMinutes))
        {
            totalMinutes = plainMinutes;
        }

        return totalMinutes > 0 ? totalMinutes : null;
    }

    private void ValidateRecipePreview(RecipeCardPreview preview)
    {
        // Required fields
        if (string.IsNullOrWhiteSpace(preview.Name))
        {
            preview.ValidationErrors.Add("Recipe name is required");
        }

        if (preview.Yield <= 0)
        {
            preview.ValidationErrors.Add("Yield must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(preview.YieldUnit))
        {
            preview.ValidationErrors.Add("Yield unit is required");
        }

        if (!preview.Ingredients.Any())
        {
            preview.ValidationErrors.Add("Recipe must have at least one ingredient");
        }

        // Warnings
        if (string.IsNullOrWhiteSpace(preview.Description))
        {
            preview.ValidationWarnings.Add("No description provided");
        }

        if (string.IsNullOrWhiteSpace(preview.Category))
        {
            preview.ValidationWarnings.Add("No category assigned");
        }

        if (preview.Ingredients.Any(i => !i.IsMatched))
        {
            var unmatchedCount = preview.Ingredients.Count(i => !i.IsMatched);
            var unmatchedNames = string.Join(", ", preview.Ingredients.Where(i => !i.IsMatched).Select(i => i.IngredientName).Take(3));
            if (unmatchedCount > 3)
            {
                unmatchedNames += $"... ({unmatchedCount - 3} more)";
            }
            preview.ValidationWarnings.Add($"⚠️ {unmatchedCount} missing ingredient(s) will be imported with $0.00 cost: {unmatchedNames}");
            preview.ValidationWarnings.Add("Add these ingredients to your database to enable cost calculation");
        }

        preview.IsValid = !preview.ValidationErrors.Any();
    }

    private class CellData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsBold { get; set; }
    }

    private class IngredientTableHeader
    {
        public int Row { get; set; }
        public int QuantityCol { get; set; }
        public int UnitCol { get; set; }
        public int IngredientCol { get; set; }
        public bool IsCombinedQuantityUnit { get; set; }
        public bool SkipFirstRow { get; set; }
    }

    private class ParsedIngredient
    {
        public decimal Quantity { get; set; }
        public UnitType Unit { get; set; }
        public string IngredientName { get; set; } = string.Empty;
    }
}
