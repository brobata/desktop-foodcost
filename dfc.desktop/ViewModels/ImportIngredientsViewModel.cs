using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class ImportIngredientsViewModel : ViewModelBase
{
    private readonly IExcelImportService _excelImportService;
    private readonly IIngredientService _ingredientService;
    private readonly Action _onImportSuccess;
    private readonly Action _onCancel;
    private readonly Guid _locationId;
    private readonly IStorageProvider? _storageProvider;

    [ObservableProperty]
    private string? _selectedFilePath;

    [ObservableProperty]
    private ObservableCollection<Ingredient> _previewIngredients = new();

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<string> _errors = new();

    [ObservableProperty]
    private bool _canImport;

    public string ImportSummary =>
        PreviewIngredients.Any()
            ? $"{PreviewIngredients.Count} ingredient(s) ready to import"
            : string.Empty;

    public ImportIngredientsViewModel(
        IExcelImportService excelImportService,
        IIngredientService ingredientService,
        Action onImportSuccess,
        Action onCancel,
        Guid locationId,
        IStorageProvider? storageProvider = null)
    {
        _excelImportService = excelImportService;
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
            Title = "Select Excel File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Excel Files")
                {
                    Patterns = new[] { "*.xlsx", "*.xls" }
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
            await LoadPreviewAsync();
        }
    }

    private async Task LoadPreviewAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
            return;

        try
        {
            ErrorMessage = null;
            StatusMessage = "Loading...";
            Errors.Clear();
            PreviewIngredients.Clear();
            CanImport = false;

            var result = await _excelImportService.ImportIngredientsFromExcelAsync(SelectedFilePath, _locationId);

            if (result.Success)
            {
                foreach (var ingredient in result.Ingredients)
                {
                    PreviewIngredients.Add(ingredient);
                }

                StatusMessage = $"Found {result.SuccessCount} valid ingredient(s)";

                if (result.Errors.Any())
                {
                    ErrorMessage = $"{result.Errors.Count} row(s) had errors and were skipped:";
                    foreach (var error in result.Errors.Take(10)) // Show first 10 errors
                    {
                        Errors.Add(error);
                    }

                    if (result.Errors.Count > 10)
                    {
                        Errors.Add($"... and {result.Errors.Count - 10} more error(s)");
                    }
                }

                CanImport = PreviewIngredients.Any();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to load file";
                StatusMessage = null;
                CanImport = false;
            }

            OnPropertyChanged(nameof(ImportSummary));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading file: {ex.Message}";
            StatusMessage = null;
            CanImport = false;
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        if (!PreviewIngredients.Any())
            return;

        try
        {
            StatusMessage = $"Importing {PreviewIngredients.Count} ingredient(s)...";
            ErrorMessage = null;

            int successCount = 0;
            var importErrors = new List<string>();

            foreach (var ingredient in PreviewIngredients)
            {
                try
                {
                    // Check for duplicates by searching for exact name match
                    var existing = await _ingredientService.SearchIngredientsAsync(ingredient.Name, _locationId);
                    if (existing.Any(i => i.Name.Equals(ingredient.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        importErrors.Add($"Skipped '{ingredient.Name}' - already exists");
                        continue;
                    }

                    await _ingredientService.CreateIngredientAsync(ingredient);
                    successCount++;
                }
                catch (Exception ex)
                {
                    importErrors.Add($"Failed to import '{ingredient.Name}': {ex.Message}");
                }
            }

            if (successCount > 0)
            {
                StatusMessage = $"Successfully imported {successCount} of {PreviewIngredients.Count} ingredient(s)";

                if (importErrors.Any())
                {
                    ErrorMessage = $"{importErrors.Count} ingredient(s) were skipped:";
                    Errors.Clear();
                    foreach (var error in importErrors.Take(10))
                    {
                        Errors.Add(error);
                    }

                    if (importErrors.Count > 10)
                    {
                        Errors.Add($"... and {importErrors.Count - 10} more");
                    }
                }

                // Wait a moment to show the success message
                await Task.Delay(1500);
                _onImportSuccess();
            }
            else
            {
                ErrorMessage = "No ingredients were imported";
                if (importErrors.Any())
                {
                    Errors.Clear();
                    foreach (var error in importErrors.Take(10))
                    {
                        Errors.Add(error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Import failed: {ex.Message}";
            StatusMessage = null;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel();
    }
}
