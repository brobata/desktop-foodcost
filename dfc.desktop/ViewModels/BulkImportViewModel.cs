using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Dfc.Desktop.Models;
using OfficeOpenXml;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class BulkImportViewModel : ViewModelBase
{
    private readonly IImportMapService _importMapService;
    private readonly IIngredientService _ingredientService;
    private readonly Action _onImportSuccess;
    private readonly Action _onCancel;
    private readonly Guid _locationId;
    private readonly IStorageProvider? _storageProvider;
    private ImportMap? _detectedMap;

    [ObservableProperty]
    private string? _selectedFilePath;

    [ObservableProperty]
    private string? _detectedVendor;

    [ObservableProperty]
    private string? _detectionInfo;

    [ObservableProperty]
    private ObservableCollection<IngredientPreviewModel> _previewIngredients = new();

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<string> _errors = new();

    [ObservableProperty]
    private ObservableCollection<string> _warnings = new();

    [ObservableProperty]
    private bool _canImport;

    [ObservableProperty]
    private int _validCount;

    [ObservableProperty]
    private int _warningCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private int _totalRows;

    public string ImportSummary =>
        PreviewIngredients.Any()
            ? $"{ValidCount} valid, {WarningCount} warnings, {ErrorCount} errors out of {TotalRows} total rows"
            : string.Empty;

    public string ValidationSummary =>
        TotalRows > 0
            ? $"Validation: {ValidCount} valid | {WarningCount} warnings | {ErrorCount} errors"
            : string.Empty;

    public BulkImportViewModel(
        IImportMapService importMapService,
        IIngredientService ingredientService,
        Action onImportSuccess,
        Action onCancel,
        Guid locationId,
        IStorageProvider? storageProvider = null)
    {
        _importMapService = importMapService;
        _ingredientService = ingredientService;
        _onImportSuccess = onImportSuccess;
        _onCancel = onCancel;
        _locationId = locationId;
        _storageProvider = storageProvider;
    }

    [RelayCommand]
    private async Task BrowseFile()
    {
        if (_storageProvider == null) return;

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Vendor File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Vendor Files")
                {
                    Patterns = new[] { "*.csv", "*.xlsx", "*.xls" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        });

        if (files.Count > 0)
        {
            SelectedFilePath = files[0].Path.LocalPath;
            await AutoDetectAndPreviewAsync();
        }
    }

    private async Task AutoDetectAndPreviewAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
            return;

        try
        {
            ErrorMessage = null;
            DetectedVendor = null;
            DetectionInfo = null;
            Errors.Clear();
            PreviewIngredients.Clear();
            CanImport = false;

            // Read headers to auto-detect vendor
            var headers = GetFileHeaders(SelectedFilePath);
            _detectedMap = _importMapService.AutoDetectMap(headers, _locationId);

            if (_detectedMap != null)
            {
                DetectedVendor = _detectedMap.VendorName;
                DetectionInfo = $"Using {_detectedMap.MapName} import configuration";

                // Load preview with detected map
                var result = await _importMapService.ImportWithMapAsync(SelectedFilePath, _detectedMap, _locationId);

                if (result.Success)
                {
                    TotalRows = result.ImportItems.Count + result.Errors.Count;
                    ValidCount = 0;
                    WarningCount = 0;
                    ErrorCount = result.Errors.Count;
                    Warnings.Clear();

                    // Validate each imported ingredient and add to preview
                    foreach (var item in result.ImportItems.Take(100)) // Preview first 100
                    {
                        var validationResult = await ValidateImportedIngredient(item.Ingredient);

                        var preview = new IngredientPreviewModel(
                            item.Ingredient,
                            item.CasePrice,
                            item.TotalQuantity)
                        {
                            HasErrors = !validationResult.IsValid,
                            HasWarnings = validationResult.Warnings.Any(),
                            ValidationErrors = string.Join("; ", validationResult.Errors.Select(e => e.Message)),
                            ValidationWarnings = string.Join("; ", validationResult.Warnings.Select(w => w.Message))
                        };

                        PreviewIngredients.Add(preview);

                        if (!validationResult.IsValid)
                        {
                            // Already counted in ErrorCount
                        }
                        else if (validationResult.Warnings.Any())
                        {
                            WarningCount++;
                            // Add top warnings to the list
                            foreach (var warning in validationResult.Warnings.Take(3))
                            {
                                Warnings.Add($"{item.Ingredient.Name}: {warning.Message}");
                            }
                        }
                        else
                        {
                            ValidCount++;
                        }
                    }

                    if (result.Errors.Any())
                    {
                        ErrorMessage = $"⚠️ {result.Errors.Count} row(s) could not be processed and will be skipped during import:";
                        foreach (var error in result.Errors.Take(10))
                        {
                            Errors.Add(error);
                        }

                        if (result.Errors.Count > 10)
                        {
                            Errors.Add($"... plus {result.Errors.Count - 10} more row(s) with errors (not shown)");
                        }
                    }

                    if (Warnings.Count > 10)
                    {
                        var remaining = Warnings.Count - 10;
                        while (Warnings.Count > 10)
                        {
                            Warnings.RemoveAt(Warnings.Count - 1);
                        }
                        Warnings.Add($"... plus {remaining} more warning(s) - review individual items in the preview grid for details");
                    }

                    // Can import if we have valid ingredients (errors won't prevent import)
                    CanImport = ValidCount > 0 || WarningCount > 0;
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Failed to load preview";
                }
            }
            else
            {
                ErrorMessage = "Unable to identify vendor format from file headers.\n\n" +
                              "Supported vendors:\n" +
                              "• Brothers Produce - Should contain columns: Item Number, Name/Quantity/UOM, Price\n" +
                              "• Sysco - Should contain columns: SUPC, Case $, Split $\n" +
                              "• US Foods - Should contain: Product Number, Product Description\n" +
                              "• Ben E Keith - Should contain: Item #, Weekly Case Average\n" +
                              "• Desktop Food Cost Export - Contains: Current Price, Case Quantity, Vendor SKU\n\n" +
                              "Please verify your file is from a supported vendor or use the Desktop Food Cost export format.";
            }

            OnPropertyChanged(nameof(ImportSummary));
            OnPropertyChanged(nameof(ValidationSummary));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to read file: {ex.Message}\n\n" +
                          "Common causes:\n" +
                          "• File is open in another program (close Excel/CSV editor and try again)\n" +
                          "• File is corrupted or not a valid CSV/Excel file\n" +
                          "• Insufficient permissions to read the file";
            CanImport = false;
        }
    }

    private List<string> GetFileHeaders(string filePath)
    {
        var headers = new List<string>();

        try
        {
            if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                // Try different delimiters
                var delimiters = new[] { ",", "\t", "|", ";" };
                foreach (var delimiter in delimiters)
                {
                    try
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            Delimiter = delimiter,
                            HasHeaderRecord = true
                        };

                        using var reader = new StreamReader(filePath);
                        using var csv = new CsvReader(reader, config);

                        csv.Read();
                        csv.ReadHeader();
                        if (csv.HeaderRecord != null && csv.HeaderRecord.Length > 0)
                        {
                            return csv.HeaderRecord.ToList();
                        }
                    }
                    catch
                    {
                        // Try next delimiter
                    }
                }
            }
            else if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                     filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet != null)
                {
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        headers.Add(worksheet.Cells[1, col].Text?.Trim() ?? $"Column{col}");
                    }
                }
            }
        }
        catch
        {
            // Return empty if we can't read headers
        }

        return headers;
    }

    [RelayCommand]
    private async Task Import()
    {
        if (!PreviewIngredients.Any() || _detectedMap == null)
            return;

        try
        {
            ErrorMessage = null;

            // Re-import all ingredients (not just preview)
            var result = await _importMapService.ImportWithMapAsync(SelectedFilePath!, _detectedMap, _locationId);

            int successCount = 0;
            int updatedCount = 0;
            var importErrors = new List<string>();

            foreach (var ingredient in result.Ingredients)
            {
                try
                {
                    // Check if ingredient exists by SKU
                    Ingredient? existing = null;
                    if (!string.IsNullOrWhiteSpace(ingredient.VendorSku))
                    {
                        existing = await _ingredientService.GetIngredientBySkuAsync(ingredient.VendorSku, _locationId);
                    }

                    if (existing != null)
                    {
                        // Update price only - keep user's unit and quantity settings
                        // Recalculate unit price: new case price / existing case quantity
                        if (ingredient.CurrentPrice > 0 && existing.CaseQuantity > 0)
                        {
                            // The imported ingredient has the new case price already divided by its quantity
                            // We need to recalculate based on the existing case quantity
                            var newCasePrice = ingredient.CurrentPrice * ingredient.CaseQuantity;
                            existing.CurrentPrice = newCasePrice / existing.CaseQuantity;
                            existing.ModifiedAt = DateTime.UtcNow;

                            await _ingredientService.UpdateIngredientAsync(existing);
                            updatedCount++;
                        }
                    }
                    else
                    {
                        // New ingredient - create it
                        await _ingredientService.CreateIngredientAsync(ingredient);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    var errorDetails = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorDetails += $" Inner: {ex.InnerException.Message}";
                        if (ex.InnerException.InnerException != null)
                        {
                            errorDetails += $" Details: {ex.InnerException.InnerException.Message}";
                        }
                    }
                    importErrors.Add($"Failed to import '{ingredient.Name}': {errorDetails}");
                }
            }

            var totalProcessed = successCount + updatedCount;

            if (totalProcessed > 0)
            {
                // Show success message
                var messages = new List<string>();
                if (successCount > 0)
                    messages.Add($"{successCount} new");
                if (updatedCount > 0)
                    messages.Add($"{updatedCount} updated");

                DetectionInfo = $"✓ Imported {string.Join(", ", messages)} ingredient(s)";

                // Show brief success message then close
                await Task.Delay(1000);
                _onImportSuccess();
            }
            else
            {
                if (importErrors.Any())
                {
                    ErrorMessage = $"Import failed - {importErrors.Count} error(s) occurred:";
                    Errors.Clear();
                    foreach (var error in importErrors.Take(10))
                    {
                        Errors.Add(error);
                    }
                    if (importErrors.Count > 10)
                    {
                        Errors.Add($"... plus {importErrors.Count - 10} more error(s)");
                    }
                }
                else
                {
                    ErrorMessage = "No ingredients were imported. Please verify the file contains valid ingredient data.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error during import: {ex.Message}\n\nPlease check the file format and try again. If the problem persists, contact support.";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel();
    }

    private async Task<ImportValidationResult> ValidateImportedIngredient(Ingredient ingredient)
    {
        var result = new ImportValidationResult();

        // Name validation
        if (string.IsNullOrWhiteSpace(ingredient.Name))
        {
            result.AddError("Name", "Missing ingredient name - this row will be skipped during import");
        }
        else if (ingredient.Name.Length < 2)
        {
            result.AddError("Name", $"Name '{ingredient.Name}' is too short (minimum 2 characters required)");
        }
        else if (ingredient.Name.Length > 200)
        {
            result.AddWarning("Name", $"Name is very long ({ingredient.Name.Length} chars) - consider shortening for better display");
        }

        // Price validation
        if (ingredient.CurrentPrice < 0)
        {
            result.AddError("Price", $"Invalid price ${ingredient.CurrentPrice:F2} - price cannot be negative");
        }
        else if (ingredient.CurrentPrice == 0)
        {
            result.AddWarning("Price", "Price is $0.00 - ingredient will show zero cost in recipes. Consider adding pricing later.");
        }
        else if (ingredient.CurrentPrice > 10000)
        {
            result.AddWarning("Price", $"Unusually high unit price: ${ingredient.CurrentPrice:F2} - please verify this is correct");
        }
        else if (ingredient.CurrentPrice < 0.01m)
        {
            result.AddWarning("Price", $"Very low unit price: ${ingredient.CurrentPrice:F4} - verify this is the correct unit price");
        }

        // Quantity validation
        if (ingredient.CaseQuantity <= 0)
        {
            result.AddError("Quantity", $"Invalid case quantity: {ingredient.CaseQuantity} - must be greater than 0");
        }
        else if (ingredient.CaseQuantity > 10000)
        {
            result.AddWarning("Quantity", $"Very large case quantity: {ingredient.CaseQuantity:F0} {ingredient.Unit} - please verify this is accurate");
        }
        else if (ingredient.CaseQuantity < 0.01m)
        {
            result.AddWarning("Quantity", $"Very small case quantity: {ingredient.CaseQuantity:F4} {ingredient.Unit} - verify this is correct");
        }

        // Vendor validation
        if (string.IsNullOrWhiteSpace(ingredient.VendorName))
        {
            result.AddWarning("Vendor", "No vendor name - this will make it harder to track supplier information");
        }

        // SKU validation
        if (string.IsNullOrWhiteSpace(ingredient.VendorSku))
        {
            result.AddWarning("SKU", "No vendor SKU - future imports won't be able to update this ingredient's price automatically");
        }

        // Category validation
        if (string.IsNullOrWhiteSpace(ingredient.Category))
        {
            result.AddWarning("Category", "No category assigned - ingredient will appear in 'Uncategorized'. Consider organizing items by category.");
        }

        // Check for duplicate by SKU if available
        if (!string.IsNullOrWhiteSpace(ingredient.VendorSku))
        {
            try
            {
                var existing = await _ingredientService.GetIngredientBySkuAsync(ingredient.VendorSku, _locationId);
                if (existing != null)
                {
                    var priceDiff = ingredient.CurrentPrice - existing.CurrentPrice;
                    var priceDiffPercent = existing.CurrentPrice > 0
                        ? (priceDiff / existing.CurrentPrice) * 100
                        : 0;

                    if (Math.Abs(priceDiffPercent) > 20)
                    {
                        result.AddWarning("Duplicate",
                            $"SKU exists - price will update from ${existing.CurrentPrice:F2} to ${ingredient.CurrentPrice:F2} ({priceDiffPercent:+0.0;-0.0}% change)");
                    }
                    else
                    {
                        result.AddWarning("Duplicate",
                            $"SKU exists - price will update from ${existing.CurrentPrice:F2} to ${ingredient.CurrentPrice:F2}");
                    }
                }
            }
            catch
            {
                // Ignore errors during duplicate check
            }
        }

        return result;
    }
}

public class ImportValidationResult
{
    public List<ImportValidationError> Errors { get; } = new();
    public List<ImportValidationError> Warnings { get; } = new();
    public bool IsValid => !Errors.Any();

    public void AddError(string field, string message)
    {
        Errors.Add(new ImportValidationError(field, message));
    }

    public void AddWarning(string field, string message)
    {
        Warnings.Add(new ImportValidationError(field, message));
    }
}

public record ImportValidationError(string Field, string Message);
