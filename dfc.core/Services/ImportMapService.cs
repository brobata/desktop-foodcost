using Dfc.Core.Enums;
using Dfc.Core.Models;
using OfficeOpenXml;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IImportMapService
{
    // Existing methods
    List<ImportMap> GetDefaultMaps(Guid locationId);
    ImportMap? AutoDetectMap(List<string> headers, Guid locationId);
    Task<BulkImportResult> ImportWithMapAsync(string filePath, ImportMap map, Guid locationId);

    // File Analysis
    Task<FilePreviewData> AnalyzeFileAsync(string filePath, int headerRow = 1, int sampleRows = 5);
    List<HeaderRowOption> DetectHeaderRowOptions(string filePath, int maxRows = 10);
    FileColumn AnalyzeColumn(string header, List<string> values);
    QuantityParseMode DetectParseMode(List<string> sampleValues);

    // Preview Generation
    Task<List<ImportPreviewItem>> GeneratePreviewAsync(
        FilePreviewData file,
        ImportMap mapping,
        Guid locationId,
        Func<string, Guid, Task<Ingredient?>> findBySku);
}

public class ImportMapService : IImportMapService
{
    private readonly IGlobalConfigService _globalConfigService;

    public ImportMapService(IGlobalConfigService globalConfigService)
    {
        _globalConfigService = globalConfigService;
        ExcelPackage.License.SetNonCommercialOrganization("Desktop Food Cost");
    }

    public List<ImportMap> GetDefaultMaps(Guid locationId)
    {
        var maps = new List<ImportMap>();

        // FIRST: Try to load maps from Firebase global configuration
        var globalMaps = _globalConfigService.GetVendorMaps();
        if (globalMaps != null && globalMaps.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[ImportMapService] Loading {globalMaps.Count} vendor maps from Firebase");
            foreach (var globalMap in globalMaps)
            {
                // Convert GlobalVendorMap to ImportMap
                var parseMode = QuantityParseMode.Separate; // Default
                if (!string.IsNullOrEmpty(globalMap.ParseMode))
                {
                    Enum.TryParse<QuantityParseMode>(globalMap.ParseMode, out parseMode);
                }

                maps.Add(new ImportMap
                {
                    Id = Guid.NewGuid(),
                    MapName = globalMap.MapName,
                    VendorName = globalMap.VendorName ?? string.Empty,
                    DetectionPattern = globalMap.DetectionPattern ?? string.Empty,
                    Delimiter = globalMap.Delimiter,
                    HeaderRow = globalMap.HeaderRow,
                    NameColumn = globalMap.NameColumn,
                    BrandColumn = globalMap.BrandColumn,
                    PriceColumn = globalMap.PriceColumn,
                    SkuColumn = globalMap.SkuColumn,
                    CategoryColumn = globalMap.CategoryColumn,
                    ParseMode = parseMode,
                    PackColumn = globalMap.PackColumn,
                    SizeColumn = globalMap.SizeColumn,
                    UnitColumn = globalMap.UnitColumn,
                    CombinedQuantityColumn = globalMap.CombinedQuantityColumn,
                    SplitCharacter = globalMap.SplitCharacter ?? string.Empty,
                    PriceIsPerUnitColumn = globalMap.PriceIsPerUnitColumn,
                    VendorColumn = globalMap.VendorColumn,
                    IsSystemMap = globalMap.IsSystemMap,
                    LocationId = locationId
                });
            }

            return maps;
        }

        // FALLBACK: If Firebase maps not available, use hardcoded defaults
        System.Diagnostics.Debug.WriteLine("[ImportMapService] Firebase vendor maps not available - using hardcoded defaults");

        return new List<ImportMap>
        {
            // Desktop Food Cost Export Map - Check this first!
            new ImportMap
            {
                Id = Guid.NewGuid(),
                MapName = "Desktop Food Cost Export",
                VendorName = "Desktop Food Cost",
                DetectionPattern = "Current Price,Case Quantity,Vendor SKU",
                Delimiter = ",",
                HeaderRow = 1,
                NameColumn = "Name",
                CategoryColumn = "Category",
                PriceColumn = "Current Price",
                SkuColumn = "Vendor SKU",
                ParseMode = QuantityParseMode.Separate,
                PackColumn = "Case Quantity",
                SizeColumn = "1", // Case Quantity is already the total, so size is 1
                UnitColumn = "Unit",
                VendorColumn = "Vendor Name",
                IsSystemMap = true,
                LocationId = locationId
            },

            // Brothers Produce Map - 3-row format
            new ImportMap
            {
                Id = Guid.NewGuid(),
                MapName = "Brothers Produce",
                VendorName = "Brothers Produce",
                DetectionPattern = "Item Number,Name/Quantity/UOM,Price",
                Delimiter = ",",
                HeaderRow = 1,
                NameColumn = "Name/Quantity/UOM",  // Will be in column B of row N+1
                PriceColumn = "Price",              // Will be in column C of row N
                SkuColumn = "Item Number",          // Will be in column A of row N
                CategoryColumn = null,
                ParseMode = QuantityParseMode.BrothersProduceThreeRow,
                CombinedQuantityColumn = "Name/Quantity/UOM", // Contains combined name + qty/uom
                IsSystemMap = true,
                LocationId = locationId
            },

            // Sysco Map
            new ImportMap
            {
                Id = Guid.NewGuid(),
                MapName = "Sysco",
                VendorName = "Sysco",
                DetectionPattern = "SUPC,Case $,Split $",
                Delimiter = ",",
                HeaderRow = 1,
                NameColumn = "Desc",
                BrandColumn = "Brand",
                PriceColumn = "Case $",
                SkuColumn = "SUPC",
                CategoryColumn = "Cat",
                ParseMode = QuantityParseMode.Separate,
                PackColumn = "Pack",
                SizeColumn = "Size",
                UnitColumn = "Unit",
                PriceIsPerUnitColumn = "Per Lb",
                IsSystemMap = true,
                LocationId = locationId
            },

            // US Foods Map
            new ImportMap
            {
                Id = Guid.NewGuid(),
                MapName = "US Foods",
                VendorName = "US Foods",
                DetectionPattern = "Product Number,Product Description,Product Brand",
                Delimiter = ",",
                HeaderRow = 1,
                NameColumn = "Product Description",
                BrandColumn = "Product Brand",
                PriceColumn = "Product Price",
                SkuColumn = "Product Number",
                CategoryColumn = "USF Class Description",
                ParseMode = QuantityParseMode.Combined,
                CombinedQuantityColumn = "Product Package Size",
                SplitCharacter = "/",
                IsSystemMap = true,
                LocationId = locationId
            },

            // Ben E Keith Map
            new ImportMap
            {
                Id = Guid.NewGuid(),
                MapName = "Ben E Keith",
                VendorName = "Ben E Keith",
                DetectionPattern = "Item #,Weekly Case Average,Label Name",
                Delimiter = ",",
                HeaderRow = 1,
                NameColumn = "Item Name",
                BrandColumn = "Brand",
                PriceColumn = "Price",
                SkuColumn = "Item #",
                CategoryColumn = "Temp Zone",
                ParseMode = QuantityParseMode.Combined,
                CombinedQuantityColumn = "Pack / Size",
                SplitCharacter = "/",
                IsSystemMap = true,
                LocationId = locationId
            }
        };
    }

    public ImportMap? AutoDetectMap(List<string> headers, Guid locationId)
    {
        var defaultMaps = GetDefaultMaps(locationId);
        var headerString = string.Join(",", headers.Select(h => h.Trim()));

        foreach (var map in defaultMaps)
        {
            var patterns = map.DetectionPattern.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var matchCount = patterns.Count(pattern =>
                headers.Any(h => h.Contains(pattern.Trim(), StringComparison.OrdinalIgnoreCase)));

            // If most patterns match, use this map
            if (matchCount >= patterns.Length * 0.7) // 70% match threshold
            {
                return map;
            }
        }

        return null;
    }

    public async Task<BulkImportResult> ImportWithMapAsync(string filePath, ImportMap map, Guid locationId)
    {
        return await Task.Run(() =>
        {
            var result = new BulkImportResult();

            try
            {
                List<Dictionary<string, string>> rows;

                if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    rows = ParseCsvFile(filePath, map);
                }
                else if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    rows = ParseExcelFile(filePath, map);
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Unsupported file format. Use .csv or .xlsx";
                    return result;
                }

                // Parse rows into ingredients
                foreach (var row in rows)
                {
                    try
                    {
                        var importItem = ParseRow(row, map, locationId);
                        if (importItem != null)
                        {
                            result.Ingredients.Add(importItem.Ingredient);
                            result.ImportItems.Add(importItem);
                            result.SuccessCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row error: {ex.Message}");
                    }
                }

                result.Success = result.Ingredients.Any();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Import failed: {ex.Message}";
            }

            return result;
        });
    }

    #region File Analysis Methods

    public async Task<FilePreviewData> AnalyzeFileAsync(string filePath, int headerRow = 1, int sampleRows = 5)
    {
        return await Task.Run(() =>
        {
            var result = new FilePreviewData
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                DetectedHeaderRow = headerRow
            };

            try
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    result.ErrorMessage = "File not found";
                    return result;
                }

                result.FileSizeBytes = fileInfo.Length;

                // Check file size limit (10MB)
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    result.ErrorMessage = "File is too large. Maximum size is 10MB.";
                    return result;
                }

                if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    AnalyzeCsvFile(filePath, result, headerRow, sampleRows);
                }
                else if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                         filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    AnalyzeExcelFile(filePath, result, headerRow, sampleRows);
                }
                else
                {
                    result.ErrorMessage = "Unsupported file format. Use .csv or .xlsx";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error reading file: {ex.Message}";
            }

            return result;
        });
    }

    private void AnalyzeCsvFile(string filePath, FilePreviewData result, int headerRow, int sampleRows)
    {
        // Detect delimiter
        result.DetectedDelimiter = DetectCsvDelimiter(filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = result.DetectedDelimiter,
            HasHeaderRecord = false, // We'll handle headers manually
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
            BadDataFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var allRows = new List<List<string>>();
        while (csv.Read())
        {
            var row = new List<string>();
            for (int i = 0; csv.TryGetField(i, out string? value); i++)
            {
                row.Add(value ?? string.Empty);
            }
            allRows.Add(row);
        }

        result.TotalRowCount = Math.Max(0, allRows.Count - headerRow);

        if (allRows.Count >= headerRow)
        {
            result.Headers = allRows[headerRow - 1];

            // Get sample rows
            for (int i = headerRow; i < Math.Min(allRows.Count, headerRow + sampleRows); i++)
            {
                result.SampleRows.Add(allRows[i]);
            }
        }

        // Analyze columns
        AnalyzeColumns(result);
    }

    private void AnalyzeExcelFile(string filePath, FilePreviewData result, int headerRow, int sampleRows)
    {
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet?.Dimension == null)
        {
            result.ErrorMessage = "No data found in Excel file";
            return;
        }

        result.TotalRowCount = Math.Max(0, worksheet.Dimension.End.Row - headerRow);

        // Get headers
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            var header = worksheet.Cells[headerRow, col].Text?.Trim() ?? $"Column{col}";
            result.Headers.Add(header);
        }

        // Get sample rows
        for (int row = headerRow + 1; row <= Math.Min(worksheet.Dimension.End.Row, headerRow + sampleRows); row++)
        {
            var rowData = new List<string>();
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                rowData.Add(worksheet.Cells[row, col].Text?.Trim() ?? string.Empty);
            }
            result.SampleRows.Add(rowData);
        }

        // Analyze columns
        AnalyzeColumns(result);
    }

    private string DetectCsvDelimiter(string filePath)
    {
        var delimiters = new[] { ",", "\t", ";", "|" };
        var firstLine = File.ReadLines(filePath).FirstOrDefault() ?? "";

        // Count occurrences of each delimiter
        var counts = delimiters.Select(d => (delimiter: d, count: firstLine.Split(d).Length - 1)).ToList();

        // Return delimiter with most occurrences (minimum 1)
        var best = counts.OrderByDescending(x => x.count).FirstOrDefault();
        return best.count > 0 ? best.delimiter : ",";
    }

    private void AnalyzeColumns(FilePreviewData result)
    {
        for (int i = 0; i < result.Headers.Count; i++)
        {
            var sampleValues = result.SampleRows
                .Where(row => row.Count > i)
                .Select(row => row[i])
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Take(10)
                .ToList();

            var column = AnalyzeColumn(result.Headers[i], sampleValues);
            column.Index = i;
            result.Columns.Add(column);
        }
    }

    public List<HeaderRowOption> DetectHeaderRowOptions(string filePath, int maxRows = 10)
    {
        var options = new List<HeaderRowOption>();

        try
        {
            List<List<string>> rows;

            if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                rows = ReadCsvRows(filePath, maxRows);
            }
            else
            {
                rows = ReadExcelRows(filePath, maxRows);
            }

            int bestRow = 1;
            double bestScore = 0;

            for (int i = 0; i < Math.Min(rows.Count, maxRows); i++)
            {
                var row = rows[i];
                var score = CalculateHeaderLikelihood(row);

                var option = new HeaderRowOption
                {
                    RowNumber = i + 1,
                    Values = row,
                    Preview = string.Join(", ", row.Take(5)) + (row.Count > 5 ? "..." : ""),
                    IsDetected = false
                };

                if (score > bestScore)
                {
                    bestScore = score;
                    bestRow = i + 1;
                }

                options.Add(option);
            }

            // Mark the best row as detected
            var detected = options.FirstOrDefault(o => o.RowNumber == bestRow);
            if (detected != null)
                detected.IsDetected = true;
        }
        catch
        {
            // Return empty list on error
        }

        return options;
    }

    private List<List<string>> ReadCsvRows(string filePath, int maxRows)
    {
        var rows = new List<List<string>>();
        var delimiter = DetectCsvDelimiter(filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = false,
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
            BadDataFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        while (csv.Read() && rows.Count < maxRows)
        {
            var row = new List<string>();
            for (int i = 0; csv.TryGetField(i, out string? value); i++)
            {
                row.Add(value ?? string.Empty);
            }
            rows.Add(row);
        }

        return rows;
    }

    private List<List<string>> ReadExcelRows(string filePath, int maxRows)
    {
        var rows = new List<List<string>>();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet?.Dimension == null)
            return rows;

        for (int row = 1; row <= Math.Min(worksheet.Dimension.End.Row, maxRows); row++)
        {
            var rowData = new List<string>();
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                rowData.Add(worksheet.Cells[row, col].Text?.Trim() ?? string.Empty);
            }
            rows.Add(rowData);
        }

        return rows;
    }

    private double CalculateHeaderLikelihood(List<string> row)
    {
        if (row.Count == 0 || row.All(string.IsNullOrWhiteSpace))
            return 0;

        double score = 0;
        int nonEmptyCount = row.Count(v => !string.IsNullOrWhiteSpace(v));

        // Headers typically have many non-empty cells
        score += nonEmptyCount * 0.1;

        // Headers are typically text, not numbers
        int textCount = row.Count(v => !string.IsNullOrWhiteSpace(v) && !Regex.IsMatch(v, @"^[\d\.\$\,\-]+$"));
        score += textCount * 0.2;

        // Headers often contain common column names
        var commonHeaders = new[] { "name", "price", "sku", "unit", "brand", "desc", "category", "product", "item", "vendor", "cost", "pack", "size", "qty", "quantity" };
        int matchCount = row.Count(v => commonHeaders.Any(h => v.ToLowerInvariant().Contains(h)));
        score += matchCount * 0.5;

        // Headers typically don't contain prices
        int priceCount = row.Count(v => Regex.IsMatch(v, @"^\$?\d+\.\d{2}$"));
        score -= priceCount * 0.3;

        return score;
    }

    public FileColumn AnalyzeColumn(string header, List<string> values)
    {
        var column = new FileColumn
        {
            Name = header,
            SampleValues = values.Take(5).ToList()
        };

        if (values.Count == 0)
            return column;

        var headerLower = header.ToLowerInvariant();

        // Check if looks like number
        column.LooksLikeNumber = values.Count(v => Regex.IsMatch(v, @"^[\d\.\,\-]+$")) > values.Count * 0.7;

        // Check if looks like price
        column.LooksLikePrice = values.Count(v => Regex.IsMatch(v, @"^\$?[\d\,]+\.?\d*$")) > values.Count * 0.7 ||
                                headerLower.Contains("price") || headerLower.Contains("cost") || headerLower.Contains("$");

        // Check if looks like SKU
        column.LooksLikeSku = (headerLower.Contains("sku") || headerLower.Contains("item") ||
                              headerLower.Contains("product") || headerLower.Contains("number") ||
                              headerLower.Contains("supc") || headerLower.Contains("upc")) &&
                             values.All(v => v.Length >= 3 && v.Length <= 20);

        // Check if looks like combined quantity (e.g., "6/5 LB")
        column.LooksLikeCombinedQuantity = values.Count(v => Regex.IsMatch(v, @"^\d+\s*/\s*\d+")) > values.Count * 0.5 ||
                                           (headerLower.Contains("pack") && headerLower.Contains("size"));

        // Check if looks like unit
        var unitPatterns = new[] { "lb", "oz", "kg", "g", "ml", "l", "ea", "each", "ct", "count", "doz", "gal", "qt", "pt", "cup" };
        column.LooksLikeUnit = values.Count(v => unitPatterns.Any(u => v.ToLowerInvariant().Contains(u))) > values.Count * 0.5 ||
                              headerLower == "unit" || headerLower == "uom";

        // Suggest mapping based on analysis
        column.SuggestedMapping = SuggestMapping(header, column);
        column.DetectionConfidence = CalculateConfidence(column);

        return column;
    }

    private string? SuggestMapping(string header, FileColumn column)
    {
        var headerLower = header.ToLowerInvariant();

        // Name suggestions
        if (headerLower.Contains("desc") || headerLower.Contains("name") ||
            headerLower.Contains("product") && !headerLower.Contains("number"))
            return "Name";

        // Price suggestions
        if (column.LooksLikePrice || headerLower.Contains("price") || headerLower.Contains("cost"))
            return "Price";

        // SKU suggestions
        if (column.LooksLikeSku || headerLower.Contains("sku") || headerLower.Contains("supc") ||
            headerLower.Contains("item #") || headerLower.Contains("item number") ||
            headerLower.Contains("product number"))
            return "SKU";

        // Quantity/Pack suggestions
        if (column.LooksLikeCombinedQuantity)
            return "CombinedQuantity";

        if (headerLower.Contains("pack"))
            return "Pack";

        if (headerLower.Contains("size") && !headerLower.Contains("pack"))
            return "Size";

        // Unit suggestions
        if (column.LooksLikeUnit || headerLower == "unit" || headerLower == "uom")
            return "Unit";

        // Brand suggestions
        if (headerLower.Contains("brand"))
            return "Brand";

        // Vendor suggestions
        if (headerLower.Contains("vendor"))
            return "Vendor";

        // Category suggestions
        if (headerLower.Contains("cat") || headerLower.Contains("category") || headerLower.Contains("class"))
            return "Category";

        return null;
    }

    private double CalculateConfidence(FileColumn column)
    {
        if (column.SuggestedMapping == null)
            return 0;

        double confidence = 0.5; // Base confidence

        if (column.LooksLikePrice && column.SuggestedMapping == "Price")
            confidence = 0.9;
        else if (column.LooksLikeSku && column.SuggestedMapping == "SKU")
            confidence = 0.85;
        else if (column.LooksLikeCombinedQuantity && column.SuggestedMapping == "CombinedQuantity")
            confidence = 0.9;
        else if (column.LooksLikeUnit && column.SuggestedMapping == "Unit")
            confidence = 0.85;

        return confidence;
    }

    public QuantityParseMode DetectParseMode(List<string> sampleValues)
    {
        if (sampleValues.Count == 0)
            return QuantityParseMode.Separate;

        // Check for combined format (e.g., "6/5 LB")
        int combinedCount = sampleValues.Count(v => Regex.IsMatch(v, @"^\d+\s*/\s*\d+"));
        if (combinedCount > sampleValues.Count * 0.5)
            return QuantityParseMode.Combined;

        return QuantityParseMode.Separate;
    }

    #endregion

    #region Preview Generation

    public async Task<List<ImportPreviewItem>> GeneratePreviewAsync(
        FilePreviewData file,
        ImportMap mapping,
        Guid locationId,
        Func<string, Guid, Task<Ingredient?>> findBySku)
    {
        var items = new List<ImportPreviewItem>();

        // Parse all rows from the file
        List<Dictionary<string, string>> rows;

        if (file.FilePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            rows = ParseCsvFile(file.FilePath, mapping);
        }
        else
        {
            rows = ParseExcelFile(file.FilePath, mapping);
        }

        int rowNumber = mapping.HeaderRow + 1;

        foreach (var row in rows)
        {
            var item = await ParseRowToPreviewItemAsync(row, rowNumber, mapping, locationId, findBySku);
            items.Add(item);
            rowNumber++;
        }

        return items;
    }

    private async Task<ImportPreviewItem> ParseRowToPreviewItemAsync(
        Dictionary<string, string> row,
        int rowNumber,
        ImportMap map,
        Guid locationId,
        Func<string, Guid, Task<Ingredient?>> findBySku)
    {
        var item = new ImportPreviewItem { RowNumber = rowNumber };

        try
        {
            // Extract name
            var name = GetValue(row, map.NameColumn);
            if (string.IsNullOrWhiteSpace(name))
            {
                item.HasError = true;
                item.ErrorMessage = "Name is required";
                item.Status = ImportPreviewStatus.Error;
                return item;
            }

            // For Brothers Produce, extract just the product name
            if (map.ParseMode == QuantityParseMode.BrothersProduceThreeRow)
            {
                name = ExtractBrothersProduceName(name);
            }

            // Extract brand and prepend to name if needed
            var brand = GetValue(row, map.BrandColumn);
            if (!string.IsNullOrWhiteSpace(brand) && !name.Contains(brand, StringComparison.OrdinalIgnoreCase))
            {
                name = $"{brand} {name}".Trim();
            }

            item.Name = name;
            item.Brand = brand;

            // Extract SKU
            item.Sku = GetValue(row, map.SkuColumn);

            // Extract price
            var priceText = GetValue(row, map.PriceColumn);
            if (!string.IsNullOrWhiteSpace(priceText))
            {
                var cleanPrice = Regex.Replace(priceText, @"[^\d\.]", "");
                if (decimal.TryParse(cleanPrice, out var casePrice))
                {
                    item.CasePrice = casePrice;
                }
            }

            // Parse quantity and unit
            var (quantity, unit) = ParseQuantityAndUnit(row, map);
            item.Quantity = quantity > 0 ? quantity : 1;
            item.Unit = unit;
            item.UnitText = unit.ToString();

            // Calculate unit price
            if (item.CasePrice.HasValue && item.Quantity.HasValue && item.Quantity.Value > 0)
            {
                // Check if price is already per unit
                var priceIsPerUnit = false;
                if (!string.IsNullOrEmpty(map.PriceIsPerUnitColumn))
                {
                    var perUnitValue = GetValue(row, map.PriceIsPerUnitColumn);
                    priceIsPerUnit = perUnitValue?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false;
                }

                item.Price = priceIsPerUnit ? item.CasePrice : item.CasePrice / item.Quantity;
            }

            // Extract vendor and category
            item.Vendor = GetValue(row, map.VendorColumn) ?? map.VendorName;
            item.Category = GetValue(row, map.CategoryColumn) ??
                           (map.ParseMode == QuantityParseMode.BrothersProduceThreeRow ? "Produce" : null);

            // Check for warnings
            ValidatePreviewItem(item);

            // Check for existing ingredient by SKU
            if (!string.IsNullOrWhiteSpace(item.Sku) && findBySku != null)
            {
                var existing = await findBySku(item.Sku, locationId);
                if (existing != null)
                {
                    item.ExistingIngredientId = existing.Id;
                    item.ExistingPrice = existing.CurrentPrice;
                    item.ExistingName = existing.Name;

                    // Check if there are any changes
                    if (item.Price.HasValue && Math.Abs(item.Price.Value - existing.CurrentPrice) > 0.001m)
                    {
                        item.Status = ImportPreviewStatus.Update;
                        item.StatusMessage = $"UPDATE (was ${existing.CurrentPrice:F2})";
                    }
                    else
                    {
                        item.Status = ImportPreviewStatus.NoChange;
                        item.StatusMessage = "No changes";
                    }
                }
                else
                {
                    item.Status = ImportPreviewStatus.New;
                    item.StatusMessage = "NEW";
                }
            }
            else if (string.IsNullOrWhiteSpace(item.Sku))
            {
                // No SKU - warn but still allow as new
                item.HasWarning = true;
                item.WarningMessage = "No SKU - cannot detect duplicates";
                item.Status = ImportPreviewStatus.New;
                item.StatusMessage = "NEW (no SKU)";
            }
            else
            {
                item.Status = ImportPreviewStatus.New;
                item.StatusMessage = "NEW";
            }
        }
        catch (Exception ex)
        {
            item.HasError = true;
            item.ErrorMessage = $"Parse error: {ex.Message}";
            item.Status = ImportPreviewStatus.Error;
        }

        return item;
    }

    private void ValidatePreviewItem(ImportPreviewItem item)
    {
        // Price validation
        if (!item.Price.HasValue || item.Price.Value == 0)
        {
            item.HasWarning = true;
            item.WarningMessage = AppendWarning(item.WarningMessage, "Price is $0.00");
        }
        else if (item.Price.Value > 500)
        {
            item.HasWarning = true;
            item.WarningMessage = AppendWarning(item.WarningMessage, $"Unusually high price (${item.Price:F2})");
        }
        else if (item.Price.Value < 0.01m)
        {
            item.HasWarning = true;
            item.WarningMessage = AppendWarning(item.WarningMessage, $"Unusually low price (${item.Price:F4})");
        }

        // Quantity validation
        if (!item.Quantity.HasValue || item.Quantity.Value <= 0)
        {
            item.HasWarning = true;
            item.WarningMessage = AppendWarning(item.WarningMessage, "Invalid quantity");
        }

        if (item.HasWarning && item.Status == ImportPreviewStatus.New)
        {
            item.Status = ImportPreviewStatus.Warning;
        }
    }

    private string? AppendWarning(string? existing, string newWarning)
    {
        if (string.IsNullOrEmpty(existing))
            return newWarning;
        return $"{existing}; {newWarning}";
    }

    #endregion

    private List<Dictionary<string, string>> ParseCsvFile(string filePath, ImportMap map)
    {
        var rows = new List<Dictionary<string, string>>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = map.Delimiter,
            HasHeaderRecord = true,
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
            BadDataFound = null // Ignore bad data
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        // Skip to header row
        for (int i = 1; i < map.HeaderRow; i++)
        {
            csv.Read();
        }

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord?.ToList() ?? new List<string>();

        while (csv.Read())
        {
            var row = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                row[header] = csv.GetField(header) ?? string.Empty;
            }
            rows.Add(row);
        }

        return rows;
    }

    private List<Dictionary<string, string>> ParseExcelFile(string filePath, ImportMap map)
    {
        var rows = new List<Dictionary<string, string>>();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return rows;

        // Get headers
        var headers = new List<string>();
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            headers.Add(worksheet.Cells[map.HeaderRow, col].Text?.Trim() ?? $"Column{col}");
        }

        // Special handling for Brothers Produce 3-row format
        if (map.ParseMode == QuantityParseMode.BrothersProduceThreeRow)
        {
            // Process every 3 rows (data row, name row, empty row)
            for (int row = map.HeaderRow + 1; row <= worksheet.Dimension.End.Row; row += 3)
            {
                var rowDict = new Dictionary<string, string>();

                // Row N: Get SKU (column A) and Price (column C)
                var sku = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;
                var price = worksheet.Cells[row, 3].Text?.Trim() ?? string.Empty;

                // Row N+1: Get Name/Quantity/UOM (column B)
                var nameQtyUom = string.Empty;
                if (row + 1 <= worksheet.Dimension.End.Row)
                {
                    nameQtyUom = worksheet.Cells[row + 1, 2].Text?.Trim() ?? string.Empty;
                }

                // Skip if essential data is missing
                if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(price) || string.IsNullOrWhiteSpace(nameQtyUom))
                    continue;

                // Map to standard column names
                rowDict["Item Number"] = sku;
                rowDict["Price"] = price;
                rowDict["Name/Quantity/UOM"] = nameQtyUom;

                rows.Add(rowDict);
            }
        }
        else
        {
            // Standard row-per-item format
            for (int row = map.HeaderRow + 1; row <= worksheet.Dimension.End.Row; row++)
            {
                var rowDict = new Dictionary<string, string>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = headers[col - 1];
                    rowDict[header] = worksheet.Cells[row, col].Text?.Trim() ?? string.Empty;
                }
                rows.Add(rowDict);
            }
        }

        return rows;
    }

    private IngredientImportItem? ParseRow(Dictionary<string, string> row, ImportMap map, Guid locationId)
    {
        // Extract name
        var name = GetValue(row, map.NameColumn);
        if (string.IsNullOrWhiteSpace(name))
            return null;

        // For Brothers Produce, extract just the product name (without quantity/UOM)
        if (map.ParseMode == QuantityParseMode.BrothersProduceThreeRow)
        {
            name = ExtractBrothersProduceName(name);
        }

        // Extract price
        var priceText = GetValue(row, map.PriceColumn);
        var hasPrice = !string.IsNullOrWhiteSpace(priceText);

        decimal casePrice = 0;
        if (hasPrice && priceText != null)
        {
            // Clean price (remove $, commas)
            priceText = Regex.Replace(priceText, @"[^\d\.]", "");
            if (!decimal.TryParse(priceText, out casePrice))
                hasPrice = false;
        }

        // Parse quantity and unit
        (decimal quantity, UnitType unit) = ParseQuantityAndUnit(row, map);

        if (quantity <= 0)
            return null;

        // Calculate unit price
        decimal unitPrice = 0;
        if (hasPrice)
        {
            // Check if price is already per unit (e.g., Sysco's "Per Lb" column = "Y")
            var priceIsPerUnit = false;
            if (!string.IsNullOrEmpty(map.PriceIsPerUnitColumn))
            {
                var perUnitValue = GetValue(row, map.PriceIsPerUnitColumn);
                priceIsPerUnit = perUnitValue?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false;
            }

            // If price is for entire case, divide by quantity to get unit price
            unitPrice = priceIsPerUnit ? casePrice : casePrice / quantity;
        }

        // Create ingredient
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CurrentPrice = unitPrice,
            Unit = unit,
            CaseQuantity = quantity,
            LocationId = locationId,
            VendorName = GetValue(row, map.VendorColumn) ?? map.VendorName,
            VendorSku = GetValue(row, map.SkuColumn),
            Category = GetValue(row, map.CategoryColumn) ?? (map.ParseMode == QuantityParseMode.BrothersProduceThreeRow ? "Produce" : null),
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        // Add brand to name if available
        var brand = GetValue(row, map.BrandColumn);
        if (!string.IsNullOrWhiteSpace(brand) && !name.Contains(brand, StringComparison.OrdinalIgnoreCase))
        {
            ingredient.Name = $"{brand} {name}".Trim();
        }

        return new IngredientImportItem
        {
            Ingredient = ingredient,
            CasePrice = casePrice,
            TotalQuantity = quantity
        };
    }

    private (decimal quantity, UnitType unit) ParseQuantityAndUnit(Dictionary<string, string> row, ImportMap map)
    {
        if (map.ParseMode == QuantityParseMode.BrothersProduceThreeRow && !string.IsNullOrEmpty(map.CombinedQuantityColumn))
        {
            var nameQtyUom = GetValue(row, map.CombinedQuantityColumn);
            return ParseBrothersProduceQuantity(nameQtyUom);
        }
        else if (map.ParseMode == QuantityParseMode.Combined && !string.IsNullOrEmpty(map.CombinedQuantityColumn))
        {
            var combined = GetValue(row, map.CombinedQuantityColumn);
            return ParseCombinedQuantity(combined, map.SplitCharacter);
        }
        else if (map.ParseMode == QuantityParseMode.Separate)
        {
            var packStr = GetValue(row, map.PackColumn);

            // For SizeColumn, check if it's a literal value (like "1") or a column name
            string? sizeStr = null;
            if (!string.IsNullOrEmpty(map.SizeColumn))
            {
                // If SizeColumn contains only digits and decimals, treat as literal value
                if (Regex.IsMatch(map.SizeColumn, @"^[\d\.]+$"))
                {
                    sizeStr = map.SizeColumn; // Use literal value
                }
                else
                {
                    sizeStr = GetValue(row, map.SizeColumn); // Look up column
                }
            }

            var unitStr = GetValue(row, map.UnitColumn);
            return ParseSeparateQuantity(packStr, sizeStr, unitStr);
        }

        return (0, UnitType.Ounce);
    }

    private string ExtractBrothersProduceName(string nameQtyUom)
    {
        if (string.IsNullOrWhiteSpace(nameQtyUom))
            return string.Empty;

        // Format examples: "ARUGULA BABY CASE 4 LB" → "ARUGULA BABY CASE"
        //                  "SPICE BAY LEAF WHOLE 12oz" → "SPICE BAY LEAF WHOLE"
        //                  "CELERY 36 CT" → "CELERY"

        // Try to match: [name] [quantity] [unit]
        var match = Regex.Match(nameQtyUom, @"^(.+?)\s+(\d+\.?\d*)\s*([A-Z]+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Try alternate pattern with CASE: "ITEM NAME CASE 4 LB" → "ITEM NAME CASE"
        match = Regex.Match(nameQtyUom, @"^(.+?)\s+CASE\s+(\d+\.?\d*)\s*([A-Z]+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim() + " CASE";
        }

        // If no match, return original (shouldn't happen with valid data)
        return nameQtyUom.Trim();
    }

    private (decimal quantity, UnitType unit) ParseBrothersProduceQuantity(string? nameQtyUom)
    {
        if (string.IsNullOrWhiteSpace(nameQtyUom))
            return (0, UnitType.Ounce);

        // Format examples: "ARUGULA BABY CASE 4 LB", "SPICE BAY LEAF WHOLE 12oz", "CELERY 36 CT"
        // Strategy: Find last number in string, everything after is unit

        // Try to match: [name] [quantity] [unit]
        var match = Regex.Match(nameQtyUom, @"(.+?)\s+(\d+\.?\d*)\s*([A-Z]+)$", RegexOptions.IgnoreCase);
        if (match.Success && decimal.TryParse(match.Groups[2].Value, out var quantity))
        {
            var unitText = match.Groups[3].Value;
            if (TryParseUnit(unitText, out var unit))
            {
                return (quantity, unit);
            }
        }

        // Try alternate pattern with CASE: "ITEM NAME CASE 4 LB"
        match = Regex.Match(nameQtyUom, @"(.+?)\s+CASE\s+(\d+\.?\d*)\s*([A-Z]+)", RegexOptions.IgnoreCase);
        if (match.Success && decimal.TryParse(match.Groups[2].Value, out quantity))
        {
            var unitText = match.Groups[3].Value;
            if (TryParseUnit(unitText, out var unit))
            {
                return (quantity, unit);
            }
        }

        // Fallback: couldn't parse, return 1 Each
        return (1, UnitType.Each);
    }

    private (decimal quantity, UnitType unit) ParseCombinedQuantity(string? combined, string splitChar)
    {
        if (string.IsNullOrWhiteSpace(combined))
            return (0, UnitType.Ounce);

        var parts = combined.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return (0, UnitType.Ounce);

        // Parse pack (first part)
        var packMatch = Regex.Match(parts[0].Trim(), @"[\d\.]+");
        if (!packMatch.Success || !decimal.TryParse(packMatch.Value, out var pack))
            return (0, UnitType.Ounce);

        // Parse size and unit (second part)
        var sizeMatch = Regex.Match(parts[1].Trim(), @"[\d\.]+");
        if (!sizeMatch.Success || !decimal.TryParse(sizeMatch.Value, out var size))
            return (0, UnitType.Ounce);

        var unitText = Regex.Replace(parts[1].Trim(), @"[\d\.\s]+", "").Trim();
        if (!TryParseUnit(unitText, out var unit))
            unit = UnitType.Ounce;

        return (pack * size, unit);
    }

    private (decimal quantity, UnitType unit) ParseSeparateQuantity(string? packStr, string? sizeStr, string? unitStr)
    {
        if (string.IsNullOrWhiteSpace(packStr))
            return (0, UnitType.Ounce);

        // Parse pack
        var packMatch = Regex.Match(packStr, @"[\d\.]+");
        if (!packMatch.Success || !decimal.TryParse(packMatch.Value, out var pack))
            return (0, UnitType.Ounce);

        // Parse size - handle both column values and literal values (e.g., "1")
        decimal size = 1; // Default to 1 if no size or if size is missing
        if (!string.IsNullOrWhiteSpace(sizeStr))
        {
            var sizeMatch = Regex.Match(sizeStr, @"[\d\.]+");
            if (sizeMatch.Success && decimal.TryParse(sizeMatch.Value, out var parsedSize))
            {
                size = parsedSize;
            }
        }

        // Extract unit from unit column or size string
        UnitType unit = UnitType.Ounce;
        if (!string.IsNullOrWhiteSpace(unitStr))
        {
            TryParseUnit(unitStr, out unit);
        }
        else if (!string.IsNullOrWhiteSpace(sizeStr))
        {
            // Try to extract unit from size string
            var unitText = Regex.Replace(sizeStr, @"[\d\.\s]+", "").Trim();
            if (!string.IsNullOrWhiteSpace(unitText))
            {
                TryParseUnit(unitText, out unit);
            }
        }

        return (pack * size, unit);
    }

    private bool TryParseUnit(string unitText, out UnitType unit)
    {
        unit = UnitType.Ounce;

        if (string.IsNullOrWhiteSpace(unitText))
            return false;

        var normalized = unitText.Trim().ToLowerInvariant()
            .Replace(" ", "")
            .Replace("-", "");

        // FIRST: Try Firebase global unit mappings
        var globalMapping = _globalConfigService.GetUnitMapping(unitText);
        if (!string.IsNullOrEmpty(globalMapping) && Enum.TryParse<UnitType>(globalMapping, out var mappedUnit))
        {
            unit = mappedUnit;
            return true;
        }

        // FALLBACK: Use hardcoded mappings
        unit = normalized switch
        {
            "tsp" or "teaspoon" or "teaspoons" => UnitType.Teaspoon,
            "tbsp" or "tablespoon" or "tablespoons" => UnitType.Tablespoon,
            "floz" or "fl.oz" or "fluidounce" or "fluidounces" => UnitType.FluidOunce,
            "cup" or "cups" or "c" or "cp" => UnitType.Cup,
            "pint" or "pints" or "pt" => UnitType.Pint,
            "quart" or "quarts" or "qt" => UnitType.Quart,
            "gallon" or "gallons" or "gal" or "ga" => UnitType.Gallon,
            "oz" or "ounce" or "ounces" => UnitType.Ounce,
            "lb" or "lbs" or "pound" or "pounds" or "#" => UnitType.Pound,
            "ml" or "milliliter" or "milliliters" => UnitType.Milliliter,
            "l" or "liter" or "liters" => UnitType.Liter,
            "g" or "gram" or "grams" => UnitType.Gram,
            "kg" or "kilogram" or "kilograms" => UnitType.Kilogram,
            "ea" or "each" or "piece" or "pieces" => UnitType.Each,
            "ct" or "count" => UnitType.Count,
            "doz" or "dozen" or "dozens" => UnitType.Dozen,
            _ => UnitType.Ounce
        };

        return true;
    }

    private string? GetValue(Dictionary<string, string> row, string? columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            return null;

        return row.TryGetValue(columnName, out var value) ? value : null;
    }
}

public class BulkImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Ingredient> Ingredients { get; set; } = new();
    public List<IngredientImportItem> ImportItems { get; set; } = new();
    public int SuccessCount { get; set; }
}

public class IngredientImportItem
{
    public Ingredient Ingredient { get; set; } = null!;
    public decimal CasePrice { get; set; }
    public decimal TotalQuantity { get; set; }
}
