using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Core.Services;
using Dfc.Desktop.Models;
using Dfc.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Dfc.Desktop.ViewModels;

public partial class ImportMapperViewModel : ViewModelBase
{
    private readonly IImportMapService _importMapService;
    private readonly IImportMapRepository _importMapRepository;
    private readonly IImportBatchRepository _importBatchRepository;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IIngredientService _ingredientService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IStatusNotificationService _notificationService;
    private readonly IStorageProvider? _storageProvider;
    private readonly Timer _undoTimer;
    private readonly Action? _onClose;
    private readonly Action? _onImportSuccess;

    #region File State

    [ObservableProperty]
    private FilePreviewData? _fileData;

    [ObservableProperty]
    private string? _selectedFilePath;

    [ObservableProperty]
    private bool _isFileLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _loadingMessage;

    #endregion

    #region Mapping State

    [ObservableProperty]
    private ImportMap? _currentMapping;

    [ObservableProperty]
    private ImportMap? _matchedSavedMapping;

    [ObservableProperty]
    private bool _isEditingMapping;

    [ObservableProperty]
    private ObservableCollection<ColumnMappingItem> _columnMappings = new();

    [ObservableProperty]
    private ObservableCollection<ImportMap> _savedMappings = new();

    [ObservableProperty]
    private List<string> _availableColumns = new();

    #endregion

    #region Header Row Selection

    [ObservableProperty]
    private int _selectedHeaderRow = 1;

    [ObservableProperty]
    private ObservableCollection<HeaderRowOption> _headerRowOptions = new();

    #endregion

    #region Preview State

    [ObservableProperty]
    private ObservableCollection<ImportPreviewItem> _previewItems = new();

    [ObservableProperty]
    private int _newItemCount;

    [ObservableProperty]
    private int _updateItemCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private int _warningCount;

    [ObservableProperty]
    private int _noChangeCount;

    [ObservableProperty]
    private bool _showOnlyProblems;

    [ObservableProperty]
    private ObservableCollection<ImportPreviewItem> _filteredPreviewItems = new();

    #endregion

    #region Import Options

    [ObservableProperty]
    private bool _updateExistingItems = true;

    [ObservableProperty]
    private string _saveMappingName = string.Empty;

    [ObservableProperty]
    private bool _shouldSaveMapping;

    [ObservableProperty]
    private ObservableCollection<string> _newCategories = new();

    #endregion

    #region Import Result

    [ObservableProperty]
    private ImportBatch? _lastImportBatch;

    [ObservableProperty]
    private bool _showImportResult;

    [ObservableProperty]
    private bool _canUndo;

    [ObservableProperty]
    private string _undoTimeRemaining = string.Empty;

    [ObservableProperty]
    private int _importedNewCount;

    [ObservableProperty]
    private int _importedUpdateCount;

    [ObservableProperty]
    private int _importedSkippedCount;

    #endregion

    #region Computed Properties

    public bool CanImport => IsFileLoaded &&
                             ColumnMappings.Where(m => m.IsRequired).All(m => m.IsValid) &&
                             PreviewItems.Any(p => p.Status != ImportPreviewStatus.Error);

    public string ImportButtonText
    {
        get
        {
            var validCount = PreviewItems.Count(p => p.Status != ImportPreviewStatus.Error && p.Status != ImportPreviewStatus.NoChange);
            return $"Import {validCount} Items";
        }
    }

    public bool HasMappingMatch => MatchedSavedMapping != null;

    public string FileSummary
    {
        get
        {
            if (FileData == null) return string.Empty;
            var size = FileData.FileSizeBytes < 1024 * 1024
                ? $"{FileData.FileSizeBytes / 1024} KB"
                : $"{FileData.FileSizeBytes / (1024 * 1024):F1} MB";
            return $"{FileData.FileName} • {FileData.TotalRowCount} rows • {size}";
        }
    }

    #endregion

    public ImportMapperViewModel(
        IImportMapService importMapService,
        IImportMapRepository importMapRepository,
        IImportBatchRepository importBatchRepository,
        IIngredientRepository ingredientRepository,
        IIngredientService ingredientService,
        ICurrentLocationService currentLocationService,
        IStatusNotificationService notificationService,
        Action? onClose = null,
        IStorageProvider? storageProvider = null,
        Action? onImportSuccess = null)
    {
        _importMapService = importMapService;
        _importMapRepository = importMapRepository;
        _importBatchRepository = importBatchRepository;
        _ingredientRepository = ingredientRepository;
        _ingredientService = ingredientService;
        _currentLocationService = currentLocationService;
        _notificationService = notificationService;
        _onClose = onClose;
        _storageProvider = storageProvider;
        _onImportSuccess = onImportSuccess;

        // Initialize undo timer
        _undoTimer = new Timer(1000); // Update every second
        _undoTimer.Elapsed += OnUndoTimerElapsed;

        // Initialize column mappings
        foreach (var mapping in ColumnMappingItem.CreateFieldMappings())
        {
            ColumnMappings.Add(mapping);
        }
    }

    #region File Commands

    [RelayCommand]
    private async Task SelectFile()
    {
        if (_storageProvider == null) return;

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Import File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Spreadsheet Files")
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
            await LoadFileAsync(files[0].Path.LocalPath);
        }
    }

    [RelayCommand]
    private async Task DropFile(string filePath)
    {
        await LoadFileAsync(filePath);
    }

    [RelayCommand]
    private void ChangeFile()
    {
        // Reset state
        FileData = null;
        SelectedFilePath = null;
        IsFileLoaded = false;
        PreviewItems.Clear();
        FilteredPreviewItems.Clear();
        MatchedSavedMapping = null;
        CurrentMapping = null;
        IsEditingMapping = false;
        ShowImportResult = false;

        // Reset mappings
        foreach (var mapping in ColumnMappings)
        {
            mapping.SelectedColumn = null;
            mapping.SampleValues.Clear();
            mapping.Validate();
        }

        AvailableColumns.Clear();
    }

    private async Task LoadFileAsync(string filePath)
    {
        try
        {
            IsLoading = true;
            LoadingMessage = "Analyzing file...";

            // Detect header row options
            var headerOptions = _importMapService.DetectHeaderRowOptions(filePath);
            HeaderRowOptions.Clear();
            foreach (var option in headerOptions)
            {
                HeaderRowOptions.Add(option);
            }

            // Use detected header row
            var detectedRow = headerOptions.FirstOrDefault(o => o.IsDetected);
            SelectedHeaderRow = detectedRow?.RowNumber ?? 1;

            // Analyze file with detected header row
            FileData = await _importMapService.AnalyzeFileAsync(filePath, SelectedHeaderRow);

            if (!FileData.IsValid)
            {
                LoadingMessage = FileData.ErrorMessage;
                IsLoading = false;
                return;
            }

            SelectedFilePath = filePath;
            IsFileLoaded = true;

            // Update available columns
            AvailableColumns = new List<string> { "-- Skip --" };
            AvailableColumns.AddRange(FileData.Headers);

            // Update each mapping with available columns
            foreach (var mapping in ColumnMappings)
            {
                mapping.AvailableColumns = AvailableColumns;
            }

            // Try to match saved mapping
            LoadingMessage = "Looking for saved mappings...";
            await LoadSavedMappingsAsync();
            MatchedSavedMapping = await _importMapRepository.FindMatchingMapAsync(FileData.Headers, _currentLocationService.CurrentLocationId);

            if (MatchedSavedMapping != null)
            {
                ApplyMapping(MatchedSavedMapping);
            }
            else
            {
                // Apply auto-detection
                ApplyAutoDetection();
            }

            // Generate preview
            await RefreshPreviewAsync();

            OnPropertyChanged(nameof(FileSummary));
            OnPropertyChanged(nameof(CanImport));
            OnPropertyChanged(nameof(ImportButtonText));
            OnPropertyChanged(nameof(HasMappingMatch));
        }
        catch (Exception ex)
        {
            LoadingMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSavedMappingsAsync()
    {
        var maps = await _importMapRepository.GetUserMapsAsync(_currentLocationService.CurrentLocationId);
        SavedMappings.Clear();
        foreach (var map in maps)
        {
            SavedMappings.Add(map);
        }
    }

    #endregion

    #region Mapping Commands

    [RelayCommand]
    private async Task SelectHeaderRow(int row)
    {
        if (SelectedHeaderRow == row || string.IsNullOrEmpty(SelectedFilePath))
            return;

        SelectedHeaderRow = row;

        // Re-analyze with new header row
        FileData = await _importMapService.AnalyzeFileAsync(SelectedFilePath, row);

        if (FileData.IsValid)
        {
            // Update available columns
            AvailableColumns = new List<string> { "-- Skip --" };
            AvailableColumns.AddRange(FileData.Headers);

            foreach (var mapping in ColumnMappings)
            {
                mapping.AvailableColumns = AvailableColumns;
                mapping.SelectedColumn = null;
                mapping.Validate();
            }

            // Re-apply auto-detection
            ApplyAutoDetection();

            // Refresh preview
            await RefreshPreviewAsync();
        }

        OnPropertyChanged(nameof(FileSummary));
    }

    [RelayCommand]
    private async Task SelectColumn(ColumnMappingItem? field)
    {
        if (field == null) return;

        // Update sample values for selected column
        if (!string.IsNullOrEmpty(field.SelectedColumn) && field.SelectedColumn != "-- Skip --" && FileData != null)
        {
            var columnIndex = FileData.Headers.IndexOf(field.SelectedColumn);
            if (columnIndex >= 0)
            {
                field.SampleValues = FileData.SampleRows
                    .Where(row => row.Count > columnIndex)
                    .Select(row => row[columnIndex])
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Take(5)
                    .ToList();

                // Auto-detect parsing mode for quantity fields
                if (field.HasParsingOptions)
                {
                    var parseMode = _importMapService.DetectParseMode(field.SampleValues);
                    field.ParseMode = parseMode;

                    if (parseMode == QuantityParseMode.Combined)
                    {
                        field.DetectedFormatDescription = "Detected: Pack/Size format (e.g., 6/5 LB)";
                    }
                    else
                    {
                        field.DetectedFormatDescription = "Detected: Separate columns format";
                    }
                }
            }
        }
        else
        {
            field.SampleValues = new List<string>();
        }

        field.Validate();
        OnPropertyChanged(nameof(CanImport));

        // Refresh preview with debounce
        await RefreshPreviewAsync();
    }

    [RelayCommand]
    private void ToggleParsingOptions(ColumnMappingItem? field)
    {
        if (field != null)
        {
            field.IsParsingExpanded = !field.IsParsingExpanded;
        }
    }

    [RelayCommand]
    private async Task ApplyParsingConfig(ColumnMappingItem? field)
    {
        if (field == null) return;

        field.IsParsingExpanded = false;
        await RefreshPreviewAsync();
    }

    [RelayCommand]
    private void StartEditMapping()
    {
        IsEditingMapping = true;
    }

    [RelayCommand]
    private void CancelEditMapping()
    {
        IsEditingMapping = false;

        // Restore original mapping if we had one
        if (MatchedSavedMapping != null)
        {
            ApplyMapping(MatchedSavedMapping);
        }
    }

    [RelayCommand]
    private async Task SaveMappingChanges()
    {
        if (MatchedSavedMapping == null) return;

        // Update the mapping with current selections
        UpdateMappingFromUI(MatchedSavedMapping);
        await _importMapRepository.UpdateAsync(MatchedSavedMapping);

        IsEditingMapping = false;
    }

    [RelayCommand]
    private async Task UseDifferentMapping(ImportMap? mapping)
    {
        if (mapping == null) return;

        MatchedSavedMapping = mapping;
        ApplyMapping(mapping);
        await RefreshPreviewAsync();

        OnPropertyChanged(nameof(HasMappingMatch));
    }

    private void ApplyMapping(ImportMap map)
    {
        CurrentMapping = map;

        foreach (var field in ColumnMappings)
        {
            field.SelectedColumn = field.FieldId switch
            {
                "Name" => map.NameColumn,
                "Price" => map.PriceColumn,
                "SKU" => map.SkuColumn,
                "Quantity" => map.ParseMode == QuantityParseMode.Combined
                    ? map.CombinedQuantityColumn
                    : map.PackColumn,
                "Unit" => map.UnitColumn,
                "Brand" => map.BrandColumn,
                "Vendor" => map.VendorColumn,
                "Category" => map.CategoryColumn,
                _ => null
            };

            if (field.HasParsingOptions)
            {
                field.ParseMode = map.ParseMode;
                field.SplitCharacter = map.SplitCharacter;
            }

            field.Validate();
        }

        OnPropertyChanged(nameof(CanImport));
    }

    private void ApplyAutoDetection()
    {
        if (FileData == null) return;

        foreach (var column in FileData.Columns)
        {
            if (column.SuggestedMapping == null) continue;

            var fieldId = column.SuggestedMapping switch
            {
                "CombinedQuantity" => "Quantity",
                "Pack" => "Quantity",
                "Size" => null, // Size goes into Quantity if Pack is not separate
                _ => column.SuggestedMapping
            };

            if (fieldId == null) continue;

            var field = ColumnMappings.FirstOrDefault(f => f.FieldId == fieldId);
            if (field != null && string.IsNullOrEmpty(field.SelectedColumn))
            {
                field.SelectedColumn = column.Name;
                field.AutoDetectedColumn = column.Name;
                field.DetectionConfidence = column.DetectionConfidence;
                field.SampleValues = column.SampleValues;

                if (column.LooksLikeCombinedQuantity && field.HasParsingOptions)
                {
                    field.ParseMode = QuantityParseMode.Combined;
                    field.DetectedFormatDescription = "Detected: Pack/Size format (e.g., 6/5 LB)";
                }

                field.Validate();
            }
        }
    }

    private void UpdateMappingFromUI(ImportMap map)
    {
        foreach (var field in ColumnMappings)
        {
            var column = field.SelectedColumn == "-- Skip --" ? null : field.SelectedColumn;

            switch (field.FieldId)
            {
                case "Name":
                    map.NameColumn = column;
                    break;
                case "Price":
                    map.PriceColumn = column;
                    break;
                case "SKU":
                    map.SkuColumn = column;
                    break;
                case "Quantity":
                    if (field.ParseMode == QuantityParseMode.Combined)
                    {
                        map.CombinedQuantityColumn = column;
                        map.ParseMode = QuantityParseMode.Combined;
                        map.SplitCharacter = field.SplitCharacter;
                    }
                    else
                    {
                        map.PackColumn = column;
                        map.ParseMode = QuantityParseMode.Separate;
                    }
                    break;
                case "Unit":
                    map.UnitColumn = column;
                    break;
                case "Brand":
                    map.BrandColumn = column;
                    break;
                case "Vendor":
                    map.VendorColumn = column;
                    break;
                case "Category":
                    map.CategoryColumn = column;
                    break;
            }
        }

        map.HeaderRow = SelectedHeaderRow;
        map.DetectionPattern = string.Join(",", FileData?.Headers.Take(5) ?? Enumerable.Empty<string>());
    }

    private ImportMap CreateMappingFromUI()
    {
        var map = new ImportMap
        {
            Id = Guid.NewGuid(),
            LocationId = _currentLocationService.CurrentLocationId,
            HeaderRow = SelectedHeaderRow,
            Delimiter = FileData?.DetectedDelimiter ?? ","
        };

        UpdateMappingFromUI(map);
        return map;
    }

    #endregion

    #region Preview Commands

    private async Task RefreshPreviewAsync()
    {
        if (FileData == null || !IsFileLoaded) return;

        try
        {
            IsLoading = true;
            LoadingMessage = "Generating preview...";

            var mapping = CreateMappingFromUI();

            var items = await _importMapService.GeneratePreviewAsync(
                FileData,
                mapping,
                _currentLocationService.CurrentLocationId,
                async (sku, locId) => await _ingredientRepository.GetBySkuAsync(sku, locId));

            PreviewItems.Clear();
            foreach (var item in items)
            {
                PreviewItems.Add(item);
            }

            // Update counts
            NewItemCount = items.Count(i => i.Status == ImportPreviewStatus.New || i.Status == ImportPreviewStatus.Warning);
            UpdateItemCount = items.Count(i => i.Status == ImportPreviewStatus.Update);
            ErrorCount = items.Count(i => i.Status == ImportPreviewStatus.Error);
            WarningCount = items.Count(i => i.HasWarning);
            NoChangeCount = items.Count(i => i.Status == ImportPreviewStatus.NoChange);

            // Find new categories
            var existingCategories = (await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId))
                .Select(i => i.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToHashSet();

            NewCategories.Clear();
            var importCategories = items
                .Select(i => i.Category)
                .Where(c => !string.IsNullOrEmpty(c) && !existingCategories.Contains(c))
                .Distinct();

            foreach (var cat in importCategories)
            {
                NewCategories.Add(cat!);
            }

            ApplyPreviewFilter();

            OnPropertyChanged(nameof(CanImport));
            OnPropertyChanged(nameof(ImportButtonText));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleShowOnlyProblems()
    {
        ShowOnlyProblems = !ShowOnlyProblems;
        ApplyPreviewFilter();
    }

    private void ApplyPreviewFilter()
    {
        FilteredPreviewItems.Clear();

        var items = ShowOnlyProblems
            ? PreviewItems.Where(i => i.HasError || i.HasWarning)
            : PreviewItems;

        foreach (var item in items.Take(100)) // Limit to 100 for performance
        {
            FilteredPreviewItems.Add(item);
        }
    }

    #endregion

    #region Import Commands

    [RelayCommand]
    private async Task ExecuteImport()
    {
        if (!CanImport || FileData == null) return;

        try
        {
            IsLoading = true;
            LoadingMessage = "Importing ingredients...";

            var batch = new ImportBatch
            {
                Id = Guid.NewGuid(),
                LocationId = _currentLocationService.CurrentLocationId,
                ImportedAt = DateTime.UtcNow,
                SourceFileName = FileData.FileName,
                MappingUsed = MatchedSavedMapping?.DisplayName ?? SaveMappingName,
                UndoExpiresAt = DateTime.UtcNow.AddMinutes(5),
                CanUndo = true
            };

            var batchItems = new List<ImportBatchItem>();
            int newCount = 0, updateCount = 0, skippedCount = 0;

            foreach (var item in PreviewItems.Where(i => i.IsValid && i.Status != ImportPreviewStatus.NoChange))
            {
                try
                {
                    if (item.Status == ImportPreviewStatus.Update && item.ExistingIngredientId.HasValue)
                    {
                        if (!UpdateExistingItems)
                        {
                            skippedCount++;
                            continue;
                        }

                        // Update existing ingredient
                        var existing = await _ingredientRepository.GetByIdAsync(item.ExistingIngredientId.Value);
                        if (existing != null)
                        {
                            // Track for undo
                            batchItems.Add(new ImportBatchItem
                            {
                                Id = Guid.NewGuid(),
                                IngredientId = existing.Id,
                                Action = ImportAction.Updated,
                                PreviousPrice = existing.CurrentPrice,
                                PreviousName = existing.Name,
                                PreviousVendor = existing.VendorName,
                                PreviousCategory = existing.Category
                            });

                            // Apply updates
                            existing.CurrentPrice = item.Price ?? existing.CurrentPrice;
                            existing.ModifiedAt = DateTime.UtcNow;

                            await _ingredientService.UpdateIngredientAsync(existing);
                            updateCount++;
                        }
                    }
                    else if (item.Status == ImportPreviewStatus.New || item.Status == ImportPreviewStatus.Warning)
                    {
                        // Create new ingredient
                        var ingredient = item.ToIngredient(_currentLocationService.CurrentLocationId);
                        await _ingredientService.CreateIngredientAsync(ingredient);

                        batchItems.Add(new ImportBatchItem
                        {
                            Id = Guid.NewGuid(),
                            IngredientId = ingredient.Id,
                            Action = ImportAction.Created
                        });

                        newCount++;
                    }
                }
                catch
                {
                    skippedCount++;
                }
            }

            // Save batch
            batch.ItemCount = newCount + updateCount + skippedCount;
            batch.NewItemCount = newCount;
            batch.UpdatedItemCount = updateCount;
            batch.SkippedCount = skippedCount;

            await _importBatchRepository.AddAsync(batch);
            await _importBatchRepository.AddItemsAsync(batch.Id, batchItems);

            // Save mapping if requested
            if (ShouldSaveMapping && !string.IsNullOrWhiteSpace(SaveMappingName))
            {
                var newMap = CreateMappingFromUI();
                newMap.DisplayName = SaveMappingName;
                newMap.MapName = SaveMappingName;
                newMap.IsSavedByUser = true;
                await _importMapRepository.AddAsync(newMap);
            }

            // Update usage stats if using saved mapping
            if (MatchedSavedMapping != null)
            {
                await _importMapRepository.UpdateUsageAsync(MatchedSavedMapping.Id);
            }

            // Show result
            LastImportBatch = batch;
            ImportedNewCount = newCount;
            ImportedUpdateCount = updateCount;
            ImportedSkippedCount = skippedCount;
            CanUndo = true;
            ShowImportResult = true;

            // Start undo timer
            _undoTimer.Start();
            UpdateUndoTimeRemaining();
        }
        catch (Exception ex)
        {
            LoadingMessage = $"Import failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UndoImport()
    {
        if (LastImportBatch == null || !CanUndo) return;

        try
        {
            IsLoading = true;
            LoadingMessage = "Undoing import...";

            var batch = await _importBatchRepository.GetByIdWithItemsAsync(LastImportBatch.Id);
            if (batch == null) return;

            foreach (var item in batch.Items)
            {
                if (item.Action == ImportAction.Created)
                {
                    // Delete created ingredient
                    await _ingredientService.DeleteIngredientAsync(item.IngredientId);
                }
                else if (item.Action == ImportAction.Updated)
                {
                    // Restore previous values
                    var ingredient = await _ingredientRepository.GetByIdAsync(item.IngredientId);
                    if (ingredient != null && item.PreviousPrice.HasValue)
                    {
                        ingredient.CurrentPrice = item.PreviousPrice.Value;
                        ingredient.ModifiedAt = DateTime.UtcNow;
                        await _ingredientService.UpdateIngredientAsync(ingredient);
                    }
                }
            }

            // Delete the batch
            await _importBatchRepository.DeleteAsync(batch.Id);

            // Stop timer and reset
            _undoTimer.Stop();
            CanUndo = false;
            ShowImportResult = false;
            LastImportBatch = null;

            // Reset to file selection
            ChangeFile();

            _onImportSuccess?.Invoke();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        _undoTimer.Stop();

        if (ShowImportResult && LastImportBatch != null)
        {
            _onImportSuccess?.Invoke();
        }

        _onClose?.Invoke();
    }

    [RelayCommand]
    private void Done()
    {
        _undoTimer.Stop();
        _onImportSuccess?.Invoke();
        _onClose?.Invoke();
    }

    #endregion

    #region Undo Timer

    private void OnUndoTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (LastImportBatch == null)
        {
            _undoTimer.Stop();
            return;
        }

        var remaining = LastImportBatch.UndoExpiresAt - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            _undoTimer.Stop();
            CanUndo = false;
            UndoTimeRemaining = "Undo expired";

            // Mark as expired in database
            Task.Run(async () =>
            {
                await _importBatchRepository.ExpireUndoAsync(LastImportBatch.Id);
            });
        }
        else
        {
            UpdateUndoTimeRemaining();
        }
    }

    private void UpdateUndoTimeRemaining()
    {
        if (LastImportBatch == null) return;

        var remaining = LastImportBatch.UndoExpiresAt - DateTime.UtcNow;
        if (remaining > TimeSpan.Zero)
        {
            UndoTimeRemaining = $"{remaining.Minutes}:{remaining.Seconds:D2} remaining";
        }
    }

    #endregion
}
