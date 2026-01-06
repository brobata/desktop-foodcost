using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IEntreeCardImportService
{
    Task<EntreeCardImportResult> ParseEntreeCardsAsync(string filePath, Guid locationId);
}

public class EntreeCardImportService : IEntreeCardImportService
{
    private readonly IIngredientMatchingService _matchingService;
    private readonly IPhotoService _photoService;
    private readonly IIngredientRepository _ingredientRepository;

    public EntreeCardImportService(IIngredientMatchingService matchingService, IPhotoService photoService, IIngredientRepository ingredientRepository)
    {
        _matchingService = matchingService;
        _photoService = photoService;
        _ingredientRepository = ingredientRepository;
        // Note: EPPlus license is configured globally in App.axaml.cs
    }

    public async Task<EntreeCardImportResult> ParseEntreeCardsAsync(string filePath, Guid locationId)
    {
        var result = new EntreeCardImportResult();

        try
        {
            Console.WriteLine($"[ENTREE IMPORT] Starting import from: {filePath}");

            if (!File.Exists(filePath))
            {
                result.Success = false;
                result.ErrorMessage = "File not found";
                Console.WriteLine($"[ENTREE IMPORT] ERROR: File not found");
                return result;
            }

            Console.WriteLine($"[ENTREE IMPORT] Opening Excel file...");
            using var package = new ExcelPackage(new FileInfo(filePath));

            if (package.Workbook.Worksheets.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No worksheets found in Excel file";
                Console.WriteLine($"[ENTREE IMPORT] ERROR: No worksheets found");
                return result;
            }

            result.TotalTabs = package.Workbook.Worksheets.Count;
            Console.WriteLine($"[ENTREE IMPORT] Found {result.TotalTabs} worksheet(s)");

            // Parse each tab as an entree card
            for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
            {
                var worksheet = package.Workbook.Worksheets[i];
                Console.WriteLine($"\n[ENTREE IMPORT] ========================================");
                Console.WriteLine($"[ENTREE IMPORT] Processing worksheet {i + 1}/{package.Workbook.Worksheets.Count}: '{worksheet.Name}'");

                try
                {
                    var preview = await ParseEntreeCardTab(worksheet, i, locationId);
                    result.EntreePreviews.Add(preview);

                    if (preview.IsValid)
                    {
                        result.ValidEntrees++;
                        Console.WriteLine($"[ENTREE IMPORT] ✓ Valid entree: '{preview.Name}' ({preview.DirectIngredients.Count} ingredients, {preview.RecipeComponents.Count} recipes)");
                    }
                    else
                    {
                        result.InvalidEntrees++;
                        result.Errors.AddRange(preview.ValidationErrors.Select(e => $"{worksheet.Name}: {e}"));
                        Console.WriteLine($"[ENTREE IMPORT] ✗ Invalid entree: '{preview.Name}'");
                        Console.WriteLine($"[ENTREE IMPORT]   Validation errors:");
                        foreach (var error in preview.ValidationErrors)
                        {
                            Console.WriteLine($"[ENTREE IMPORT]     - {error}");
                        }
                    }

                    // Collect unmatched ingredients
                    foreach (var ingredient in preview.DirectIngredients.Where(i => !i.IsMatched))
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

                        unmatched.AppearsInRecipes.Add(preview.Name); // Using "AppearsInRecipes" field for entrees too
                        unmatched.UsageCount++;
                    }

                    // Collect unmatched recipes
                    foreach (var recipe in preview.RecipeComponents.Where(r => !r.IsMatched))
                    {
                        var unmatched = result.UnmatchedRecipes.FirstOrDefault(u =>
                            u.Name.Equals(recipe.RecipeName, StringComparison.OrdinalIgnoreCase));

                        if (unmatched == null)
                        {
                            unmatched = new UnmatchedRecipe
                            {
                                Name = recipe.RecipeName,
                                Suggestions = await _matchingService.GetRecipeSuggestionsAsync(recipe.RecipeName, locationId)
                            };
                            result.UnmatchedRecipes.Add(unmatched);
                        }

                        unmatched.AppearsInEntrees.Add(preview.Name);
                        unmatched.UsageCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Tab '{worksheet.Name}': {ex.Message}");
                    result.InvalidEntrees++;
                    Console.WriteLine($"[ENTREE IMPORT] ✗ EXCEPTION parsing worksheet '{worksheet.Name}': {ex.Message}");
                    Console.WriteLine($"[ENTREE IMPORT]   Stack trace: {ex.StackTrace}");
                }
            }

            result.Success = result.ValidEntrees > 0;

            if (!result.Success && result.Errors.Count == 0)
            {
                result.ErrorMessage = "No valid entrees found in file";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error reading Excel file: {ex.Message}";
        }

        return result;
    }

    private async Task<EntreeCardPreview> ParseEntreeCardTab(ExcelWorksheet worksheet, int tabIndex, Guid locationId)
    {
        Console.WriteLine($"[ENTREE IMPORT]   Parsing entree card tab...");

        var preview = new EntreeCardPreview
        {
            TabName = worksheet.Name,
            TabIndex = tabIndex
        };

        var data = ExtractWorksheetData(worksheet);
        Console.WriteLine($"[ENTREE IMPORT]   Extracted {data.Count} non-empty cells from worksheet");

        // Extract entree name (required)
        preview.Name = FindLabeledValue(data, "entree name", "name", "dish name", "plate name") ?? worksheet.Name;
        Console.WriteLine($"[ENTREE IMPORT]   Entree name: '{preview.Name}'");

        if (string.IsNullOrWhiteSpace(preview.Name) || preview.Name == worksheet.Name)
        {
            // Try first non-empty cell as name
            preview.Name = data.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.Value))?.Value ?? worksheet.Name;
        }

        // Extract metadata
        preview.Description = FindLabeledValue(data, "description", "desc", "summary");
        preview.Category = FindLabeledValue(data, "category", "type", "classification");
        preview.PlatingEquipment = FindLabeledValue(data, "plating equipment", "plating", "equipment", "service");

        // Extract menu price
        var priceText = FindLabeledValue(data, "menu price", "price", "selling price", "sell price");
        if (!string.IsNullOrWhiteSpace(priceText))
        {
            // Remove currency symbols and parse
            var cleanPrice = priceText.Replace("$", "").Replace("£", "").Replace("€", "").Trim();
            if (decimal.TryParse(cleanPrice, out decimal price))
            {
                preview.MenuPrice = price;
                Console.WriteLine($"[ENTREE IMPORT]   Menu price: ${price} (from text: '{priceText}')");
            }
        }

        // Extract direct ingredients and recipe components
        Console.WriteLine($"[ENTREE IMPORT]   Extracting components...");
        await ExtractEntreeComponents(worksheet, data, preview, locationId);
        Console.WriteLine($"[ENTREE IMPORT]   Extracted {preview.DirectIngredients.Count} ingredient(s) and {preview.RecipeComponents.Count} recipe(s)");

        // Extract photo
        Console.WriteLine($"[ENTREE IMPORT]   Extracting photo...");
        preview.PhotoUrl = await ExtractPhotoFromWorksheet(worksheet);
        if (!string.IsNullOrWhiteSpace(preview.PhotoUrl))
        {
            Console.WriteLine($"[ENTREE IMPORT]   ✓ Photo extracted: {preview.PhotoUrl}");
        }
        else
        {
            Console.WriteLine($"[ENTREE IMPORT]   No photo found in worksheet");
        }

        // Extract procedures
        Console.WriteLine($"[ENTREE IMPORT]   Extracting procedures...");
        preview.Procedures = ExtractProcedures(worksheet, data);
        Console.WriteLine($"[ENTREE IMPORT]   Extracted {preview.Procedures.Count} procedure step(s)");

        // Validation
        Console.WriteLine($"[ENTREE IMPORT]   Validating entree...");
        ValidateEntreePreview(preview);

        return preview;
    }

    private async Task ExtractEntreeComponents(
        ExcelWorksheet worksheet,
        List<CellData> data,
        EntreeCardPreview preview,
        Guid locationId)
    {
        // Look for sections: "Build", "Ingredients", "Direct Ingredients", "Recipes", or "Recipe Components"

        // Find all section headers
        var sectionHeaders = data.Where(d =>
            (d.Value.Contains("build", StringComparison.OrdinalIgnoreCase) ||
             d.Value.Contains("ingredient", StringComparison.OrdinalIgnoreCase) ||
             d.Value.Contains("recipe", StringComparison.OrdinalIgnoreCase) ||
             d.Value.Contains("component", StringComparison.OrdinalIgnoreCase)) &&
            (d.Value.EndsWith(":") || d.IsBold || d.Value.Equals("Build", StringComparison.OrdinalIgnoreCase))).ToList();

        Console.WriteLine($"[ENTREE IMPORT]   Found {sectionHeaders.Count} section header(s): {string.Join(", ", sectionHeaders.Select(s => s.Value))}");

        // Try to find "Build" section first (most common in entree cards)
        var buildTable = await ExtractComponentTable(worksheet, data, "build", locationId, isRecipe: false);
        preview.DirectIngredients = buildTable.Ingredients;
        preview.RecipeComponents = buildTable.Recipes;

        // If Build section had both, we're done
        if (preview.DirectIngredients.Any() || preview.RecipeComponents.Any())
        {
            Console.WriteLine($"[ENTREE IMPORT]   Build section processed: {preview.DirectIngredients.Count} ingredients, {preview.RecipeComponents.Count} recipes");
            return;
        }

        // Otherwise try separate sections
        // Try to find ingredient table
        var ingredientTable = await ExtractComponentTable(worksheet, data, "ingredient", locationId, isRecipe: false);
        preview.DirectIngredients = ingredientTable.Ingredients;

        // Try to find recipe table
        var recipeTable = await ExtractComponentTable(worksheet, data, "recipe", locationId, isRecipe: true);
        preview.RecipeComponents = recipeTable.Recipes;
    }

    private async Task<(List<EntreeIngredientPreview> Ingredients, List<EntreeRecipePreview> Recipes)> ExtractComponentTable(
        ExcelWorksheet worksheet,
        List<CellData> data,
        string sectionType, // "ingredient" or "recipe"
        Guid locationId,
        bool isRecipe)
    {
        var ingredients = new List<EntreeIngredientPreview>();
        var recipes = new List<EntreeRecipePreview>();

        Console.WriteLine($"[ENTREE IMPORT]     Looking for '{sectionType}' section...");

        // Find section header
        var sectionHeader = data.FirstOrDefault(d =>
            d.Value.Contains(sectionType, StringComparison.OrdinalIgnoreCase) &&
            (d.Value.EndsWith(":") || d.IsBold));

        if (sectionHeader == null)
        {
            Console.WriteLine($"[ENTREE IMPORT]     ✗ No '{sectionType}' section header found");
            return (ingredients, recipes);
        }

        Console.WriteLine($"[ENTREE IMPORT]     ✓ Found '{sectionType}' section at row {sectionHeader.Row}");

        // Look for table header below section header
        var tableHeader = FindComponentTableHeader(data, sectionHeader.Row);

        if (tableHeader == null)
        {
            Console.WriteLine($"[ENTREE IMPORT]     ✗ No table header found for '{sectionType}' section");
            return (ingredients, recipes);
        }

        // Parse rows below header
        var startRow = tableHeader.Row + 1;
        var endRow = worksheet.Dimension?.End.Row ?? startRow;
        Console.WriteLine($"[ENTREE IMPORT]     Parsing {sectionType} rows {startRow} to {endRow}");

        int consecutiveEmptyRows = 0;
        for (int row = startRow; row <= endRow; row++)
        {
            // Use .Value to get raw cell content
            var nameText = worksheet.Cells[row, tableHeader.NameCol].Value?.ToString()?.Trim();

            // Stop if we hit 3 consecutive empty names (end of table)
            if (string.IsNullOrWhiteSpace(nameText))
            {
                consecutiveEmptyRows++;
                if (consecutiveEmptyRows >= 3)
                {
                    Console.WriteLine($"[ENTREE IMPORT]       Stopping: Found {consecutiveEmptyRows} consecutive empty rows");
                    break;
                }
                continue;
            }
            consecutiveEmptyRows = 0;

            decimal quantity;
            UnitType unit;
            string originalText;

            if (tableHeader.IsCombinedQuantityUnit)
            {
                // 2-column format: AMOUNT contains "qty unit" like "2 oz" or "TT"
                // Use .Value instead of .Text to get raw cell content (handles text in number-formatted cells)
                var combinedText = worksheet.Cells[row, tableHeader.QuantityCol].Value?.ToString()?.Trim();
                Console.WriteLine($"[ENTREE IMPORT]       Row {row}: Combined='{combinedText}', Name='{nameText}'");
                System.Diagnostics.Debug.WriteLine($"[ENTREE IMPORT]       Row {row}: Combined='{combinedText}', Name='{nameText}'");

                if (string.IsNullOrWhiteSpace(combinedText))
                {
                    quantity = 1;
                    unit = UnitType.Each;
                    originalText = $"1 ea {nameText}";
                }
                else if (combinedText.Equals("TT", StringComparison.OrdinalIgnoreCase) ||
                         combinedText.Equals("T T", StringComparison.OrdinalIgnoreCase))
                {
                    // "TT" or "T T" means "to taste" - use a small quantity
                    quantity = 0.01m;
                    unit = UnitType.Teaspoon;
                    originalText = $"TT {nameText}";
                    Console.WriteLine($"[ENTREE IMPORT]         Parsed '{combinedText}' as 0.01 tsp (to taste)");
                }
                else
                {
                    // Split on whitespace: "2 oz" -> ["2", "oz"]
                    var parts = combinedText.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    string quantityText;
                    string unitText;

                    if (parts.Length >= 2)
                    {
                        quantityText = parts[0];
                        unitText = parts[1];
                    }
                    else if (parts.Length == 1)
                    {
                        // Only quantity, assume "Each"
                        quantityText = parts[0];
                        unitText = "ea";
                    }
                    else
                    {
                        quantityText = "1";
                        unitText = "ea";
                    }

                    if (!TryParseQuantity(quantityText, out quantity))
                        quantity = 1;

                    if (!TryParseUnit(unitText, out unit))
                    {
                        Console.WriteLine($"[ENTREE IMPORT]         WARNING: Failed to parse unit '{unitText}', defaulting to Each");
                        System.Diagnostics.Debug.WriteLine($"[ENTREE IMPORT]         WARNING: Failed to parse unit '{unitText}', defaulting to Each");
                        unit = UnitType.Each;
                    }

                    originalText = $"{combinedText} {nameText}";
                    Console.WriteLine($"[ENTREE IMPORT]         Parsed: Qty={quantity}, Unit={unit} (from '{quantityText}' and '{unitText}')");
                    System.Diagnostics.Debug.WriteLine($"[ENTREE IMPORT]         ✓ Parsed: Qty={quantity}, Unit={unit} (from '{quantityText}' and '{unitText}')");
                }
            }
            else
            {
                // 3-column format: separate quantity and unit columns
                // Use .Value to get raw cell content
                var quantityText = worksheet.Cells[row, tableHeader.QuantityCol].Value?.ToString()?.Trim();
                var unitText = worksheet.Cells[row, tableHeader.UnitCol].Value?.ToString()?.Trim();
                Console.WriteLine($"[ENTREE IMPORT]       Row {row}: Qty='{quantityText}', Unit='{unitText}', Name='{nameText}'");

                // Handle "T T" as "To Taste"
                if (quantityText?.Equals("T", StringComparison.Ordinal) == true &&
                    unitText?.Equals("T", StringComparison.Ordinal) == true)
                {
                    quantity = 0.01m;
                    unit = UnitType.Teaspoon;
                    originalText = $"TT {nameText}";
                    Console.WriteLine($"[ENTREE IMPORT]         Parsed 'T T' as 0.01 tsp (to taste)");
                }
                else
                {
                    if (!TryParseQuantity(quantityText, out quantity))
                        quantity = 1;

                    if (!TryParseUnit(unitText, out unit))
                    {
                        Console.WriteLine($"[ENTREE IMPORT]         WARNING: Failed to parse unit '{unitText}', defaulting to Each");
                        unit = UnitType.Each;
                    }

                    originalText = $"{quantityText} {unitText} {nameText}".Trim();
                    Console.WriteLine($"[ENTREE IMPORT]         Parsed: Qty={quantity}, Unit={unit} (from '{quantityText}' and '{unitText}')");
                }
            }

            if (isRecipe)
            {
                // Match recipe
                var matchResult = await _matchingService.FindBestRecipeMatchAsync(nameText, locationId);

                var preview = new EntreeRecipePreview
                {
                    OriginalText = originalText,
                    RecipeName = nameText,
                    Quantity = quantity,
                    Unit = unit,
                    IsMatched = matchResult.IsMatched,
                    MatchedRecipeId = matchResult.IsMatched ? matchResult.RecipeId : null,
                    MatchedRecipeName = matchResult.RecipeName,
                    MatchConfidence = matchResult.Confidence,
                    MatchMethod = matchResult.MatchMethod
                };

                recipes.Add(preview);
            }
            else
            {
                // Match ingredient
                var matchResult = await _matchingService.FindBestMatchAsync(nameText, locationId);

                // Always use the unit from the Excel file (already parsed into 'unit' variable)
                var preview = new EntreeIngredientPreview
                {
                    OriginalText = originalText,
                    IngredientName = nameText,
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
        }

        return (ingredients, recipes);
    }

    private ComponentTableHeader? FindComponentTableHeader(List<CellData> data, int afterRow)
    {
        Console.WriteLine($"[ENTREE IMPORT]       Searching for component table header after row {afterRow}...");

        // Look for header row containing quantity/unit/ingredient or quantity/unit/recipe columns
        var groupedByRow = data.Where(d => d.Row > afterRow).GroupBy(d => d.Row);

        foreach (var rowGroup in groupedByRow.Take(10)) // Search next 10 rows
        {
            var cells = rowGroup.ToList();
            Console.WriteLine($"[ENTREE IMPORT]       Checking row {rowGroup.Key}: {string.Join(", ", cells.Select(c => $"Col{c.Column}='{c.Value}'"))}");

            int? quantityCol = cells.FirstOrDefault(c =>
                c.Value.Contains("quantity", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("qty", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("amount", StringComparison.OrdinalIgnoreCase))?.Column;

            int? unitCol = cells.FirstOrDefault(c =>
                c.Value.Contains("unit", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("uom", StringComparison.OrdinalIgnoreCase))?.Column;

            int? nameCol = cells.FirstOrDefault(c =>
                c.Value.Contains("ingredient", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("recipe", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("item", StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("name", StringComparison.OrdinalIgnoreCase))?.Column;

            // If no unit column header found, check if there's a column between quantity and name
            // If quantity is in column N and name is in column N+2, then N+1 is likely the unit column
            if (!unitCol.HasValue && quantityCol.HasValue && nameCol.HasValue &&
                nameCol.Value == quantityCol.Value + 2)
            {
                // There's exactly one column between quantity and name - assume it's the unit column
                unitCol = quantityCol.Value + 1;
                Console.WriteLine($"[ENTREE IMPORT]       Detected unlabeled unit column at position {unitCol.Value} (column between quantity and name)");
            }
            else if (!unitCol.HasValue && quantityCol.HasValue && nameCol.HasValue && quantityCol.Value < nameCol.Value)
            {
                // Multiple columns between quantity and name - check which one has unit data
                for (int col = quantityCol.Value + 1; col < nameCol.Value; col++)
                {
                    var hasUnitData = data.Any(d =>
                        d.Column == col &&
                        d.Row > rowGroup.Key &&
                        d.Row <= rowGroup.Key + 10 &&
                        !string.IsNullOrWhiteSpace(d.Value) &&
                        // Check if value looks like a unit (short text, 1-5 chars)
                        d.Value.Length <= 5 &&
                        (d.Value.Equals("oz", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("lb", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("g", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("kg", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("tsp", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("tbsp", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("cup", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("floz", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("ml", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("l", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("ea", StringComparison.OrdinalIgnoreCase) ||
                         d.Value.Equals("T", StringComparison.Ordinal) || // Tablespoon (case sensitive)
                         d.Value.Equals("t", StringComparison.Ordinal))); // Teaspoon (case sensitive)

                    if (hasUnitData)
                    {
                        unitCol = col;
                        Console.WriteLine($"[ENTREE IMPORT]       Detected unlabeled unit column at position {col} (contains unit abbreviations)");
                        break;
                    }
                }
            }

            // Strategy 1: 3-column format (AMOUNT | UNIT | INGREDIENT)
            // BUT: Verify the unit column actually has data in the rows below
            if (quantityCol.HasValue && unitCol.HasValue && nameCol.HasValue)
            {
                // Check if unit column has actual data in next few rows
                var hasUnitData = data.Any(d =>
                    d.Column == unitCol.Value &&
                    d.Row > rowGroup.Key &&
                    d.Row <= rowGroup.Key + 5 &&
                    !string.IsNullOrWhiteSpace(d.Value));

                if (hasUnitData)
                {
                    Console.WriteLine($"[ENTREE IMPORT]       ✓ Found 3-column component table header at row {rowGroup.Key}");
                    Console.WriteLine($"[ENTREE IMPORT]         Quantity column: {quantityCol.Value}");
                    Console.WriteLine($"[ENTREE IMPORT]         Unit column: {unitCol.Value}");
                    Console.WriteLine($"[ENTREE IMPORT]         Name column: {nameCol.Value}");
                    return new ComponentTableHeader
                    {
                        Row = rowGroup.Key,
                        QuantityCol = quantityCol.Value,
                        UnitCol = unitCol.Value,
                        NameCol = nameCol.Value,
                        IsCombinedQuantityUnit = false
                    };
                }
                else
                {
                    Console.WriteLine($"[ENTREE IMPORT]       Found unit column header but it has no data - treating as 2-column format");
                    // Fall through to 2-column detection
                }
            }

            // Strategy 2: 2-column format (AMOUNT | INGREDIENT) where AMOUNT contains "qty unit"
            // This handles both: no unit column found, OR unit column found but empty
            if (quantityCol.HasValue && nameCol.HasValue)
            {
                Console.WriteLine($"[ENTREE IMPORT]       ✓ Found 2-column component table header at row {rowGroup.Key}");
                Console.WriteLine($"[ENTREE IMPORT]         Combined Quantity+Unit column: {quantityCol.Value}");
                Console.WriteLine($"[ENTREE IMPORT]         Name column: {nameCol.Value}");
                return new ComponentTableHeader
                {
                    Row = rowGroup.Key,
                    QuantityCol = quantityCol.Value,
                    UnitCol = -1, // No separate unit column
                    NameCol = nameCol.Value,
                    IsCombinedQuantityUnit = true
                };
            }
        }

        Console.WriteLine($"[ENTREE IMPORT]       ✗ No component table header found!");
        return null;
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

    private string? FindLabeledValue(List<CellData> data, params string[] labelVariations)
    {
        foreach (var label in labelVariations)
        {
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

                // Check adjacent cells
                var adjacentValue = data.FirstOrDefault(d =>
                    (d.Row == labelCell.Row && d.Column == labelCell.Column + 1) ||
                    (d.Row == labelCell.Row + 1 && d.Column == labelCell.Column))?.Value;

                if (!string.IsNullOrWhiteSpace(adjacentValue))
                    return adjacentValue;
            }
        }

        return null;
    }

    private bool TryParseQuantity(string? text, out decimal quantity)
    {
        quantity = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();

        // Handle fractions
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

    private bool TryParseUnit(string? unitText, out UnitType unit)
    {
        unit = UnitType.Each;

        if (string.IsNullOrWhiteSpace(unitText))
        {
            Console.WriteLine($"[ENTREE IMPORT]           TryParseUnit: Input is null/empty, returning Each");
            return false;
        }

        var trimmed = unitText.Trim();
        Console.WriteLine($"[ENTREE IMPORT]           TryParseUnit: Parsing '{trimmed}'");

        // Try exact enum match first
        if (Enum.TryParse<UnitType>(trimmed, ignoreCase: true, out unit))
        {
            Console.WriteLine($"[ENTREE IMPORT]           TryParseUnit: Matched enum directly -> {unit}");
            return true;
        }

        // Handle case-sensitive single letter abbreviations BEFORE normalization
        // In recipes: lowercase "t" = teaspoon, uppercase "T" = tablespoon
        if (trimmed == "t")
        {
            Console.WriteLine($"[ENTREE IMPORT]           TryParseUnit: Matched lowercase 't' -> Teaspoon");
            unit = UnitType.Teaspoon;
            return true;
        }
        else if (trimmed == "T")
        {
            Console.WriteLine($"[ENTREE IMPORT]           TryParseUnit: Matched uppercase 'T' -> Tablespoon");
            unit = UnitType.Tablespoon;
            return true;
        }

        // Normalize for case-insensitive matching
        var normalized = trimmed.ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace(".", "");
        Console.WriteLine($"[ENTREE IMPORT]           TryParseUnit: Normalized to '{normalized}'");

        // Match all common abbreviations and variations
        unit = normalized switch
        {
            // VOLUME - US
            // Teaspoon
            "tsp" or "teaspoon" or "teaspoons" or "tspn" or "tsps" => UnitType.Teaspoon,

            // Tablespoon
            "tbsp" or "tbs" or "tb" or "tablespoon" or "tablespoons" or "tbsps" or "tblsp" or "tblspn" => UnitType.Tablespoon,

            // Fluid Ounce
            "floz" or "fluidoz" or "fl" or "foz" or "fluidounce" or "fluidounces" or "fluoz" => UnitType.FluidOunce,

            // Cup
            "c" or "cup" or "cups" or "cp" => UnitType.Cup,

            // Pint
            "pt" or "pint" or "pints" => UnitType.Pint,

            // Quart
            "qt" or "qts" or "quart" or "quarts" => UnitType.Quart,

            // Gallon
            "gal" or "gallon" or "gallons" => UnitType.Gallon,

            // WEIGHT - US
            // Ounce
            "oz" or "ounce" or "ounces" or "ozs" => UnitType.Ounce,

            // Pound
            "lb" or "lbs" or "pound" or "pounds" or "#" or "lbm" => UnitType.Pound,

            // VOLUME - METRIC
            // Milliliter
            "ml" or "mls" or "milliliter" or "milliliters" or "millilitre" or "millilitres" => UnitType.Milliliter,

            // Liter
            "l" or "ltr" or "liter" or "liters" or "litre" or "litres" => UnitType.Liter,

            // WEIGHT - METRIC
            // Gram (most common meaning of "g")
            "g" or "gm" or "gms" or "gr" or "gram" or "grams" or "gramme" or "grammes" => UnitType.Gram,

            // Kilogram
            "kg" or "kgs" or "kilo" or "kilos" or "kilogram" or "kilograms" or "kilogramme" or "kilogrammes" => UnitType.Kilogram,

            // COUNT
            // Each
            "ea" or "each" or "piece" or "pieces" or "pc" or "pcs" or "pce" or "item" or "items" => UnitType.Each,

            // Count
            "ct" or "count" or "cnt" => UnitType.Count,

            // Dozen
            "dz" or "doz" or "dozen" or "dozens" => UnitType.Dozen,

            // Default to Each if no match
            _ => UnitType.Each
        };

        Console.WriteLine($"[ENTREE IMPORT]           TryParseUnit: Result -> {unit}");
        return true;
    }

    private async Task<string?> ExtractPhotoFromWorksheet(ExcelWorksheet worksheet)
    {
        try
        {
            // Check if worksheet has any drawings (images, shapes, etc.)
            if (worksheet.Drawings.Count == 0)
            {
                return null;
            }

            // Find all pictures in the worksheet
            var pictures = worksheet.Drawings
                .Where(d => d is OfficeOpenXml.Drawing.ExcelPicture)
                .Cast<OfficeOpenXml.Drawing.ExcelPicture>()
                .ToList();

            if (!pictures.Any())
            {
                Console.WriteLine($"[ENTREE IMPORT]     No ExcelPicture found in {worksheet.Drawings.Count} drawing(s)");
                return null;
            }

            Console.WriteLine($"[ENTREE IMPORT]     Found {pictures.Count} picture(s) in worksheet");

            // Select the LARGEST picture (actual dish photo, not logo)
            // Logos are typically small, dish photos are much larger
            var picture = pictures
                .OrderByDescending(p => p.Image.ImageBytes?.Length ?? 0)
                .First();

            Console.WriteLine($"[ENTREE IMPORT]     Selected largest picture: {picture.Name} ({picture.Image.ImageBytes?.Length ?? 0} bytes)");

            // Get the image data
            var imageBytes = picture.Image.ImageBytes;
            if (imageBytes == null || imageBytes.Length == 0)
            {
                Console.WriteLine($"[ENTREE IMPORT]     Picture has no image data");
                return null;
            }

            Console.WriteLine($"[ENTREE IMPORT]     Image size: {imageBytes.Length} bytes");

            // Save to temporary file
            var extension = GetImageExtension(picture.Image.Type ?? OfficeOpenXml.Drawing.ePictureType.Jpg);
            var tempPath = Path.Combine(Path.GetTempPath(), $"entree_photo_{Guid.NewGuid()}.{extension}");
            await File.WriteAllBytesAsync(tempPath, imageBytes);

            Console.WriteLine($"[ENTREE IMPORT]     Saved to temp file: {tempPath}");

            try
            {
                // Use PhotoService to save permanently
                var photoUrl = await _photoService.SaveEntreePhotoAsync(tempPath);
                Console.WriteLine($"[ENTREE IMPORT]     Saved permanently: {photoUrl}");
                return photoUrl;
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ENTREE IMPORT]     Error extracting photo: {ex.Message}");
            return null;
        }
    }

    private string GetImageExtension(OfficeOpenXml.Drawing.ePictureType imageType)
    {
        return imageType switch
        {
            OfficeOpenXml.Drawing.ePictureType.Jpg => "jpg",
            OfficeOpenXml.Drawing.ePictureType.Png => "png",
            OfficeOpenXml.Drawing.ePictureType.Gif => "gif",
            OfficeOpenXml.Drawing.ePictureType.Bmp => "bmp",
            OfficeOpenXml.Drawing.ePictureType.Tif => "tif",
            OfficeOpenXml.Drawing.ePictureType.Svg => "svg",
            OfficeOpenXml.Drawing.ePictureType.WebP => "webp",
            _ => "jpg" // Default to jpg
        };
    }

    private void ValidateEntreePreview(EntreeCardPreview preview)
    {
        // Required fields
        if (string.IsNullOrWhiteSpace(preview.Name))
        {
            preview.ValidationErrors.Add("Entree name is required");
        }

        if (!preview.DirectIngredients.Any() && !preview.RecipeComponents.Any())
        {
            preview.ValidationErrors.Add("Entree must have at least one ingredient or recipe component");
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

        if (!preview.MenuPrice.HasValue || preview.MenuPrice <= 0)
        {
            preview.ValidationWarnings.Add("No menu price specified");
        }

        if (preview.DirectIngredients.Any(i => !i.IsMatched))
        {
            var unmatchedCount = preview.DirectIngredients.Count(i => !i.IsMatched);
            preview.ValidationWarnings.Add($"{unmatchedCount} ingredient(s) could not be matched to database");
        }

        if (preview.RecipeComponents.Any(r => !r.IsMatched))
        {
            var unmatchedCount = preview.RecipeComponents.Count(r => !r.IsMatched);
            preview.ValidationWarnings.Add($"{unmatchedCount} recipe component(s) could not be matched to database");
        }

        preview.IsValid = !preview.ValidationErrors.Any();
    }

    private List<string> ExtractProcedures(ExcelWorksheet worksheet, List<CellData> data)
    {
        var procedures = new List<string>();

        // Find "Procedure" section header
        var procedureHeader = data.FirstOrDefault(d =>
            d.Value.Contains("procedure", StringComparison.OrdinalIgnoreCase) &&
            (d.IsBold || d.Value.EndsWith(":")));

        if (procedureHeader == null)
        {
            Console.WriteLine($"[ENTREE IMPORT]     No 'Procedure' section found");
            return procedures;
        }

        Console.WriteLine($"[ENTREE IMPORT]     Found 'Procedure' section at row {procedureHeader.Row}");

        // Procedures are typically in a 2-column format:
        // Column A (or first column after header): Step number (1, 2, 3, etc.)
        // Column B (or second column): Step description

        var startRow = procedureHeader.Row + 1;
        var endRow = worksheet.Dimension?.End.Row ?? startRow;

        // Find the column indices for step number and description
        // Look at the first data row to determine columns
        int stepNumberCol = -1;
        int descriptionCol = -1;

        // Scan a few rows after the header to find columns with numeric values and text
        for (int row = startRow; row <= Math.Min(startRow + 2, endRow); row++)
        {
            for (int col = 1; col <= Math.Min(worksheet.Dimension?.End.Column ?? 10, 10); col++)
            {
                var cellValue = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(cellValue))
                    continue;

                // Check if this looks like a step number (1-20)
                if (int.TryParse(cellValue, out int stepNum) && stepNum >= 1 && stepNum <= 20)
                {
                    stepNumberCol = col;
                    descriptionCol = col + 1; // Description is typically next column
                    Console.WriteLine($"[ENTREE IMPORT]       Detected procedure columns: Step={stepNumberCol}, Description={descriptionCol}");
                    break;
                }
            }
            if (stepNumberCol > 0)
                break;
        }

        if (stepNumberCol < 0)
        {
            Console.WriteLine($"[ENTREE IMPORT]     Could not detect procedure table structure");
            return procedures;
        }

        // Extract procedures
        int consecutiveEmptyRows = 0;
        for (int row = startRow; row <= endRow; row++)
        {
            var stepNumText = worksheet.Cells[row, stepNumberCol].Value?.ToString()?.Trim();
            var descriptionText = worksheet.Cells[row, descriptionCol].Value?.ToString()?.Trim();

            // Stop if we hit multiple empty rows
            if (string.IsNullOrWhiteSpace(stepNumText) && string.IsNullOrWhiteSpace(descriptionText))
            {
                consecutiveEmptyRows++;
                if (consecutiveEmptyRows >= 2)
                {
                    Console.WriteLine($"[ENTREE IMPORT]       Stopping: Found {consecutiveEmptyRows} consecutive empty rows");
                    break;
                }
                continue;
            }
            consecutiveEmptyRows = 0;

            // Validate step number (should be sequential or at least numeric)
            if (!string.IsNullOrWhiteSpace(stepNumText) && int.TryParse(stepNumText, out int stepNumber))
            {
                if (!string.IsNullOrWhiteSpace(descriptionText))
                {
                    procedures.Add(descriptionText);
                    Console.WriteLine($"[ENTREE IMPORT]       Step {stepNumber}: {descriptionText.Substring(0, Math.Min(50, descriptionText.Length))}...");
                }
            }
        }

        return procedures;
    }

    private class CellData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsBold { get; set; }
    }

    private class ComponentTableHeader
    {
        public int Row { get; set; }
        public int QuantityCol { get; set; }
        public int UnitCol { get; set; }
        public int NameCol { get; set; }
        public bool IsCombinedQuantityUnit { get; set; } // True if AMOUNT contains "qty unit" like "2 oz"
    }
}
