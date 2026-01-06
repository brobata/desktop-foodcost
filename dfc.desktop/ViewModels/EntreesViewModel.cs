using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Dfc.Desktop.Models;
using Dfc.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class EntreesViewModel : ViewModelBase
{
    private readonly IEntreeService _entreeService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IExcelExportService _excelExportService;
    private readonly IRecycleBinService _recycleBinService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IStatusNotificationService _notificationService;
    private readonly IUserSessionService? _sessionService;
    private readonly Func<Task>? _onItemDeleted;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<EntreeDisplayModel> _entrees;

    [ObservableProperty]
    private EntreeDisplayModel? _selectedEntree;

    [ObservableProperty]
    private ObservableCollection<EntreeDisplayModel> _selectedEntrees = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<AllergenFilterItem> _availableAllergens = new();

    [ObservableProperty]
    private int _activeFilterCount = 0;

    [ObservableProperty]
    private int _totalEntrees = 0;

    [ObservableProperty]
    private decimal _totalMenuRevenue = 0;

    [ObservableProperty]
    private decimal _averageFoodCost = 0;

    [ObservableProperty]
    private decimal _averageFoodCostPercent = 0;

    [ObservableProperty]
    private string _selectedSortOption = "Name (A-Z)";

    public List<string> SortOptions { get; } = new List<string>
    {
        "Name (A-Z)",
        "Name (Z-A)",
        "Price (Low-High)",
        "Price (High-Low)",
        "Food Cost %  (Low-High)",
        "Food Cost % (High-Low)",
        "Category (A-Z)"
    };

    [ObservableProperty]
    private string _selectedQuickFilter = "All Items";

    public List<string> QuickFilterOptions { get; } = new List<string>
    {
        "All Items",
        "High Price (>$20)",
        "Low Price (<$10)",
        "High Cost % (>40%)",
        "Low Cost % (<25%)",
        "Missing Prices",
        "No Category"
    };

    private readonly HashSet<AllergenType> _selectedAllergens = new();

    // Authentication state for UI binding
    public bool IsAuthenticated => _sessionService?.IsAuthenticated ?? false;

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadEntreesAsync();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        _ = LoadEntreesAsync();
    }

    partial void OnSelectedQuickFilterChanged(string value)
    {
        _ = LoadEntreesAsync();
    }

    public EntreesViewModel(IEntreeService entreeService, IServiceProvider serviceProvider, ICurrentLocationService currentLocationService, IStatusNotificationService notificationService, Func<Task>? onItemDeleted = null)
    {
        _entreeService = entreeService;
        _serviceProvider = serviceProvider;
        _currentLocationService = currentLocationService;
        _notificationService = notificationService;
        _sessionService = _serviceProvider.GetService(typeof(IUserSessionService)) as IUserSessionService;
        _excelExportService = _serviceProvider.GetRequiredService<IExcelExportService>();
        _recycleBinService = _serviceProvider.GetRequiredService<IRecycleBinService>();
        _onItemDeleted = onItemDeleted;
        _entrees = new ObservableCollection<EntreeDisplayModel>();
        InitializeAllergenFilters();
        _ = LoadEntreesAsync();
    }

    private void InitializeAllergenFilters()
    {
        var allergenTypes = new[]
        {
            (AllergenType.Milk, "Milk/Dairy"),
            (AllergenType.Eggs, "Eggs"),
            (AllergenType.Fish, "Fish"),
            (AllergenType.Shellfish, "Shellfish"),
            (AllergenType.TreeNuts, "Tree Nuts"),
            (AllergenType.Peanuts, "Peanuts"),
            (AllergenType.Wheat, "Wheat"),
            (AllergenType.Soybeans, "Soy"),
            (AllergenType.Sesame, "Sesame"),
            (AllergenType.GlutenFree, "Gluten-Free"),
            (AllergenType.Vegan, "Vegan"),
            (AllergenType.Vegetarian, "Vegetarian")
        };

        foreach (var (type, name) in allergenTypes)
        {
            var item = new AllergenFilterItem { Type = type, Name = name };
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AllergenFilterItem.IsSelected))
                {
                    OnAllergenFilterChanged(item);
                }
            };
            AvailableAllergens.Add(item);
        }
    }

    private void OnAllergenFilterChanged(AllergenFilterItem item)
    {
        if (item.IsSelected)
        {
            _selectedAllergens.Add(item.Type);
        }
        else
        {
            _selectedAllergens.Remove(item.Type);
        }
        ActiveFilterCount = _selectedAllergens.Count;
        _ = LoadEntreesAsync();
    }

    [RelayCommand]
    private void ClearAllFilters()
    {
        foreach (var allergen in AvailableAllergens)
        {
            allergen.IsSelected = false;
        }
        _selectedAllergens.Clear();
        ActiveFilterCount = 0;
        _ = LoadEntreesAsync();
    }

    public async Task LoadEntreesAsync()
    {
        IsLoading = true;
        try
        {
            var entrees = await _entreeService.GetAllEntreesAsync(_currentLocationService.CurrentLocationId);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                entrees = entrees.Where(e =>
                    e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Filter by allergens (if any selected)
            if (_selectedAllergens.Count > 0)
            {
                entrees = entrees.Where(e =>
                    e.EntreeAllergens != null &&
                    e.EntreeAllergens.Any(ea =>
                        ea.IsEnabled &&
                        _selectedAllergens.Contains(ea.Allergen.Type))
                ).ToList();
            }

            // Apply quick filters
            entrees = SelectedQuickFilter switch
            {
                "High Price (>$20)" => entrees.Where(e => (e.MenuPrice ?? 0) > 20).ToList(),
                "Low Price (<$10)" => entrees.Where(e => (e.MenuPrice ?? 0) < 10).ToList(),
                "High Cost % (>40%)" => entrees.Where(e => e.FoodCostPercentage > 40).ToList(),
                "Low Cost % (<25%)" => entrees.Where(e => e.FoodCostPercentage < 25).ToList(),
                "Missing Prices" => entrees.Where(e => !e.MenuPrice.HasValue || e.MenuPrice == 0).ToList(),
                "No Category" => entrees.Where(e => string.IsNullOrWhiteSpace(e.Category)).ToList(),
                _ => entrees
            };

            // Apply sorting
            entrees = SelectedSortOption switch
            {
                "Name (A-Z)" => entrees.OrderBy(e => e.Name).ToList(),
                "Name (Z-A)" => entrees.OrderByDescending(e => e.Name).ToList(),
                "Price (Low-High)" => entrees.OrderBy(e => e.MenuPrice ?? 0).ToList(),
                "Price (High-Low)" => entrees.OrderByDescending(e => e.MenuPrice ?? 0).ToList(),
                "Food Cost %  (Low-High)" => entrees.OrderBy(e => e.FoodCostPercentage).ToList(),
                "Food Cost % (High-Low)" => entrees.OrderByDescending(e => e.FoodCostPercentage).ToList(),
                "Category (A-Z)" => entrees.OrderBy(e => e.Category ?? "").ToList(),
                _ => entrees.OrderBy(e => e.Name).ToList()
            };

            Entrees.Clear();
            foreach (var entree in entrees)
            {
                Entrees.Add(new EntreeDisplayModel(entree));
            }

            // Update statistics
            TotalEntrees = Entrees.Count;
            TotalMenuRevenue = Entrees.Sum(e => e.Entree.MenuPrice ?? 0);
            AverageFoodCost = Entrees.Any()
                ? Entrees.Average(e => e.Entree.TotalCost)
                : 0;
            AverageFoodCostPercent = Entrees.Any()
                ? (decimal)Entrees.Average(e => (double)e.Entree.FoodCostPercentage)
                : 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Window? GetOwnerWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    [RelayCommand]
    private async Task AddEntree()
    {
        var owner = GetOwnerWindow();
        if (owner == null) return;

        var window = new AddEditEntreeWindow();
        var viewModel = new AddEditEntreeViewModel(
            _serviceProvider.GetRequiredService<IEntreeService>(),
            _serviceProvider.GetRequiredService<IRecipeService>(),
            _serviceProvider.GetRequiredService<IIngredientService>(),
            _serviceProvider.GetRequiredService<IRecipeCostCalculator>(),
            _serviceProvider.GetRequiredService<IPhotoService>(),
            _serviceProvider.GetRequiredService<IAllergenDetectionService>(),
            _serviceProvider.GetRequiredService<IValidationService>(),
            _currentLocationService,
            window,
            () => window.OnSaveSuccess(),
            () => window.OnCancel()
        );

        window.DataContext = viewModel;
        await window.ShowDialog(owner);

        if (window.WasSaved)
        {
            var newEntree = viewModel.GetEntree();
            if (newEntree != null)
            {
                // Add allergens from the selector control
                var selectedAllergens = window.GetAllergenSelector().GetSelectedAllergens();
                newEntree.EntreeAllergens = selectedAllergens.Select(a => new Dfc.Core.Models.EntreeAllergen
                {
                    Id = Guid.NewGuid(),
                    AllergenId = Dfc.Core.Helpers.AllergenMapper.GetAllergenId(a.AllergenType),
                    IsAutoDetected = a.IsAutoDetected,
                    IsEnabled = a.IsSelected,
                    SourceIngredients = a.SourceIngredients
                }).ToList();

                await _entreeService.CreateEntreeAsync(newEntree);
                await LoadEntreesAsync();
            }
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        var owner = GetOwnerWindow();
        if (owner == null) return;

        try
        {
            var saveDialog = await owner.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Export Entrees to Excel",
                SuggestedFileName = $"Entrees_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Excel Files")
                    {
                        Patterns = new[] { "*.xlsx" }
                    }
                }
            });

            if (saveDialog != null)
            {
                var entrees = Entrees.Select(e => e.Entree).ToList();
                await _excelExportService.ExportEntreesToExcelAsync(entrees, saveDialog.Path.LocalPath);
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Export failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ImportEntreeCards()
    {
        var owner = GetOwnerWindow();
        if (owner == null) return;

        try
        {
            var importService = _serviceProvider.GetRequiredService<IEntreeCardImportService>();
            var matchingService = _serviceProvider.GetRequiredService<IIngredientMatchingService>();
            var ingredientService = _serviceProvider.GetRequiredService<IIngredientService>();
            var recipeService = _serviceProvider.GetRequiredService<IRecipeService>();
            var mappingService = _serviceProvider.GetRequiredService<IIngredientMatchMappingService>();

            var dialog = new EntreeCardImportWindow();
            var viewModel = new EntreeCardImportViewModel(
                importService,
                _entreeService,
                matchingService,
                async () =>
                {
                    await LoadEntreesAsync();
                    dialog.Close();
                },
                () => dialog.Close(),
                _currentLocationService.CurrentLocationId,
                owner.StorageProvider,
                ingredientService,
                recipeService,
                mappingService
            );
            dialog.DataContext = viewModel;

            await dialog.ShowDialog(owner);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Entree card import failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        var owner = GetOwnerWindow();
        if (owner == null) return;

        try
        {
            var openDialog = await owner.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Import Entrees from Excel",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Excel Files")
                    {
                        Patterns = new[] { "*.xlsx" }
                    }
                }
            });

            if (openDialog != null && openDialog.Count > 0)
            {
                var filePath = openDialog[0].Path.LocalPath;
                var importedEntrees = await _excelExportService.ImportEntreesFromExcelAsync(filePath);

                // Save imported entrees
                foreach (var entree in importedEntrees)
                {
                    entree.LocationId = _currentLocationService.CurrentLocationId;
                    await _entreeService.CreateEntreeAsync(entree);
                }

                await LoadEntreesAsync();
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Import failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EditEntree(Entree? entree)
    {
        if (entree == null) return;

        var owner = GetOwnerWindow();
        if (owner == null) return;

        // Reload the entree with full navigation properties
        var fullEntree = await _entreeService.GetEntreeByIdAsync(entree.Id);
        if (fullEntree == null) return;

        var window = new AddEditEntreeWindow();
        var viewModel = new AddEditEntreeViewModel(
            _serviceProvider.GetRequiredService<IEntreeService>(),
            _serviceProvider.GetRequiredService<IRecipeService>(),
            _serviceProvider.GetRequiredService<IIngredientService>(),
            _serviceProvider.GetRequiredService<IRecipeCostCalculator>(),
            _serviceProvider.GetRequiredService<IPhotoService>(),
            _serviceProvider.GetRequiredService<IAllergenDetectionService>(),
            _serviceProvider.GetRequiredService<IValidationService>(),
            _currentLocationService,
            window,
            () => window.OnSaveSuccess(),
            () => window.OnCancel(),
            fullEntree  // Pass the fully loaded entree
        );

        window.DataContext = viewModel;
        await window.ShowDialog(owner);

        if (window.WasSaved)
        {
            var updatedEntree = viewModel.GetEntree();
            if (updatedEntree != null)
            {
                // Add allergens from the selector control
                var selectedAllergens = window.GetAllergenSelector().GetSelectedAllergens();
                updatedEntree.EntreeAllergens = selectedAllergens.Select(a => new Dfc.Core.Models.EntreeAllergen
                {
                    Id = Guid.NewGuid(),
                    AllergenId = Dfc.Core.Helpers.AllergenMapper.GetAllergenId(a.AllergenType),
                    IsAutoDetected = a.IsAutoDetected,
                    IsEnabled = a.IsSelected,
                    SourceIngredients = a.SourceIngredients
                }).ToList();

                await _entreeService.UpdateEntreeAsync(updatedEntree);
                await LoadEntreesAsync();
            }
        }
    }

    [RelayCommand]
    private async Task DeleteEntree(Entree? entree)
    {
        if (entree == null) return;

        // CRITICAL: Load a fresh, detached copy for recycle bin to avoid EF tracking conflicts
        var entreeId = entree.Id;
        var freshCopy = await _entreeService.GetEntreeByIdAsync(entreeId);
        if (freshCopy == null) return;

        // Move to recycle bin before deleting
        await _recycleBinService.MoveToRecycleBinAsync(freshCopy, DeletedItemType.Entree, _currentLocationService.CurrentLocationId);
        await _entreeService.DeleteEntreeAsync(entreeId);
        await LoadEntreesAsync();

        // Notify MainWindowViewModel to update recycle bin count
        if (_onItemDeleted != null)
            await _onItemDeleted();
    }

    [RelayCommand]
    private async Task GenerateEntreePdf(Entree? entree)
    {
        if (entree == null) return;

        try
        {
            // Fetch full entree with all relationships
            var fullEntree = await _entreeService.GetEntreeByIdAsync(entree.Id);
            if (fullEntree == null) return;

            var entreeCardService = _serviceProvider.GetRequiredService<IEntreeCardService>();
            var pdfPath = await entreeCardService.GenerateEntreeCardPdfAsync(fullEntree);

            // Open the PDF folder
            var folder = System.IO.Path.GetDirectoryName(pdfPath);
            if (!string.IsNullOrEmpty(folder))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating entree PDF: {ex}");
        }
    }

    [RelayCommand]
    private async Task DuplicateEntree(Entree? entree)
    {
        if (entree == null) return;

        // Fetch full entree with all relationships
        var fullEntree = await _entreeService.GetEntreeByIdAsync(entree.Id);
        if (fullEntree == null) return;

        // Create a copy of the entree
        var duplicate = new Entree
        {
            Name = $"{fullEntree.Name} (Copy)",
            Description = fullEntree.Description,
            Category = fullEntree.Category,
            MenuPrice = fullEntree.MenuPrice,
            PhotoUrl = fullEntree.PhotoUrl,
            PlatingEquipment = fullEntree.PlatingEquipment,
            LocationId = fullEntree.LocationId
        };

        // Copy entree recipes
        if (fullEntree.EntreeRecipes != null)
        {
            duplicate.EntreeRecipes = new List<EntreeRecipe>();
            foreach (var er in fullEntree.EntreeRecipes)
            {
                duplicate.EntreeRecipes.Add(new EntreeRecipe
                {
                    RecipeId = er.RecipeId,
                    Quantity = er.Quantity,
                    Unit = er.Unit
                });
            }
        }

        // Copy entree ingredients
        if (fullEntree.EntreeIngredients != null)
        {
            duplicate.EntreeIngredients = new List<EntreeIngredient>();
            foreach (var ei in fullEntree.EntreeIngredients)
            {
                duplicate.EntreeIngredients.Add(new EntreeIngredient
                {
                    IngredientId = ei.IngredientId,
                    Quantity = ei.Quantity,
                    Unit = ei.Unit
                });
            }
        }

        // Copy allergens
        if (fullEntree.EntreeAllergens != null)
        {
            duplicate.EntreeAllergens = new List<EntreeAllergen>();
            foreach (var allergen in fullEntree.EntreeAllergens)
            {
                duplicate.EntreeAllergens.Add(new EntreeAllergen
                {
                    AllergenId = allergen.AllergenId,
                    IsAutoDetected = allergen.IsAutoDetected,
                    IsEnabled = allergen.IsEnabled,
                    SourceIngredients = allergen.SourceIngredients
                });
            }
        }

        await _entreeService.CreateEntreeAsync(duplicate);
        await LoadEntreesAsync();
    }

    [RelayCommand]
    private async Task BatchDelete()
    {
        if (SelectedEntrees == null || SelectedEntrees.Count == 0) return;

        var owner = GetOwnerWindow();
        if (owner == null) return;

        var result = await ShowConfirmationDialog(owner,
            $"Delete {SelectedEntrees.Count} menu item(s)?",
            $"These items will be moved to the Recycle Bin.\n\n💡 You can restore deleted items from the Recycle Bin.");

        if (result)
        {
            var entreeIds = SelectedEntrees.Select(e => e.Entree.Id).ToList();
            foreach (var entreeId in entreeIds)
            {
                // CRITICAL: Load fresh copy for recycle bin to avoid EF tracking conflicts
                var freshCopy = await _entreeService.GetEntreeByIdAsync(entreeId);
                if (freshCopy != null)
                {
                    // Move to recycle bin before deleting
                    await _recycleBinService.MoveToRecycleBinAsync(freshCopy, DeletedItemType.Entree, _currentLocationService.CurrentLocationId);
                    await _entreeService.DeleteEntreeAsync(entreeId);
                }
            }
            await LoadEntreesAsync();

            // Notify MainWindowViewModel to update recycle bin count
            if (_onItemDeleted != null)
                await _onItemDeleted();
        }
    }

    [RelayCommand]
    private async Task BatchDuplicate()
    {
        if (SelectedEntrees == null || SelectedEntrees.Count == 0) return;

        foreach (var displayModel in SelectedEntrees.ToList())
        {
            await DuplicateEntree(displayModel.Entree);
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        SelectedEntrees.Clear();
        foreach (var entree in Entrees)
        {
            SelectedEntrees.Add(entree);
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedEntrees.Clear();
    }

    private async Task<bool> ShowConfirmationDialog(Window owner, string title, string message)
    {
        try
        {
            var dialog = new Window
            {
                Title = title,
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var grid = new Grid
            {
                Margin = new Thickness(20),
                RowDefinitions = new RowDefinitions("*,Auto")
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(textBlock, 0);
            grid.Children.Add(textBlock);

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Spacing = 10,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(buttonPanel, 1);

            var noButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Padding = new Thickness(10)
            };
            noButton.Click += (s, e) => dialog.Close(false);

            var yesButton = new Button
            {
                Content = "Delete",
                Width = 100,
                Background = Avalonia.Media.Brushes.Red,
                Foreground = Avalonia.Media.Brushes.White,
                Padding = new Thickness(10)
            };
            yesButton.Click += (s, e) => dialog.Close(true);

            buttonPanel.Children.Add(noButton);
            buttonPanel.Children.Add(yesButton);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;

            var result = await dialog.ShowDialog<bool>(owner);
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing confirmation dialog: {ex.Message}");
            return false; // Default to cancel on error
        }
    }
}