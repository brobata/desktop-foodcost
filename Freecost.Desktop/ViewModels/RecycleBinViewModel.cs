using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Models;
using Freecost.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class RecycleBinViewModel : ViewModelBase
{
    private readonly IRecycleBinService _recycleBinService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly ILogger<RecycleBinViewModel>? _logger;
    private readonly Action? _onItemRestored;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<DeletedItemViewModel> _deletedItems = new();

    [ObservableProperty]
    private DeletedItemViewModel? _selectedItem;

    [ObservableProperty]
    private List<string> _itemTypeFilters = new() { "All Items", "Ingredients", "Recipes", "Entrees" };

    [ObservableProperty]
    private string _selectedItemTypeFilter = "All Items";

    public RecycleBinViewModel(IRecycleBinService recycleBinService, ICurrentLocationService currentLocationService, ILogger<RecycleBinViewModel>? logger = null, Action? onItemRestored = null)
    {
        _recycleBinService = recycleBinService;
        _currentLocationService = currentLocationService;
        _logger = logger;
        _onItemRestored = onItemRestored;
    }

    public async Task LoadDeletedItemsAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[RecycleBin] LoadDeletedItemsAsync invoked");

        try
        {
            IsLoading = true;

            var currentLocationId = _currentLocationService.CurrentLocationId;
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Current location ID: {currentLocationId}");

            var deletedItems = await _recycleBinService.GetDeletedItemsAsync(currentLocationId);
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Loaded {deletedItems.Count} deleted items");

            DeletedItems.Clear();
            foreach (var item in deletedItems.OrderByDescending(x => x.DeletedDate))
            {
                DeletedItems.Add(new DeletedItemViewModel(item));
                System.Diagnostics.Debug.WriteLine($"[RecycleBin] Added item: {item.ItemType} - {item.ItemName}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error loading deleted items: {ex.Message}\n{ex.StackTrace}");
            _logger?.LogError(ex, "Error loading deleted items");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadDeletedItemsAsync();
    }

    [RelayCommand]
    private async Task RestoreItem(DeletedItemViewModel? itemViewModel)
    {
        System.Diagnostics.Debug.WriteLine($"[RecycleBin] RestoreItem command invoked");

        if (itemViewModel == null)
        {
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] RestoreItem called with null itemViewModel");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[RecycleBin] Attempting to restore: {itemViewModel.DeletedItem.ItemType} - {itemViewModel.DeletedItem.ItemName}");

        try
        {
            // Call RestoreAsync with the appropriate type based on ItemType
            BaseEntity? restored = itemViewModel.DeletedItem.ItemType switch
            {
                DeletedItemType.Ingredient => await _recycleBinService.RestoreAsync<Ingredient>(itemViewModel.DeletedItem.Id),
                DeletedItemType.Recipe => await _recycleBinService.RestoreAsync<Recipe>(itemViewModel.DeletedItem.Id),
                DeletedItemType.Entree => await _recycleBinService.RestoreAsync<Entree>(itemViewModel.DeletedItem.Id),
                _ => null
            };

            if (restored != null)
            {
                System.Diagnostics.Debug.WriteLine($"[RecycleBin] Successfully restored item, removing from list");
                DeletedItems.Remove(itemViewModel);
                _onItemRestored?.Invoke();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[RecycleBin] Failed to restore {itemViewModel.DeletedItem.ItemType}: {itemViewModel.DeletedItem.ItemName}");
                _logger?.LogError("Failed to restore {ItemType}: {ItemName}", itemViewModel.DeletedItem.ItemType, itemViewModel.DeletedItem.ItemName);
            }
        }
        catch (Exception ex)
        {
            var innerEx = ex.InnerException;
            var innerMsg = innerEx != null ? $"\nInner Exception: {innerEx.Message}" : "";
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error restoring item: {ex.Message}{innerMsg}\n{ex.StackTrace}");
            _logger?.LogError(ex, "Error restoring item: {ItemType} - {ItemName}", itemViewModel?.DeletedItem.ItemType, itemViewModel?.DeletedItem.ItemName);
        }
    }

    [RelayCommand]
    private async Task PermanentDeleteItem(DeletedItemViewModel? itemViewModel)
    {
        System.Diagnostics.Debug.WriteLine($"[RecycleBin] PermanentDeleteItem command invoked");

        if (itemViewModel == null)
        {
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] PermanentDeleteItem called with null itemViewModel");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[RecycleBin] Attempting to permanently delete: {itemViewModel.DeletedItem.ItemType} - {itemViewModel.DeletedItem.ItemName}");

        try
        {
            await _recycleBinService.PermanentlyDeleteAsync(itemViewModel.DeletedItem.Id);
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Successfully deleted item, removing from list");
            DeletedItems.Remove(itemViewModel);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error permanently deleting item: {ex.Message}\n{ex.StackTrace}");
            _logger?.LogError(ex, "Error permanently deleting item");
        }
    }

    [RelayCommand]
    private async Task EmptyRecycleBin()
    {
        System.Diagnostics.Debug.WriteLine($"[RecycleBin] EmptyRecycleBin command invoked");

        try
        {
            var currentLocationId = _currentLocationService.CurrentLocationId;
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Emptying recycle bin for location: {currentLocationId}");
            await _recycleBinService.EmptyRecycleBinAsync(currentLocationId);
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Successfully emptied recycle bin, clearing list");
            DeletedItems.Clear();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error emptying recycle bin: {ex.Message}\n{ex.StackTrace}");
            _logger?.LogError(ex, "Error emptying recycle bin");
        }
    }
}

public partial class DeletedItemViewModel : ObservableObject
{
    public DeletedItem DeletedItem { get; }

    public DeletedItemViewModel(DeletedItem deletedItem)
    {
        DeletedItem = deletedItem;
    }

    public string TypeDisplay => DeletedItem.ItemType.ToString();

    public string EntityName => DeletedItem.ItemName ?? "Unknown";

    public string DeletedAtDisplay => DeletedItem.DeletedDate.ToString("g");

    public string ExpirationDisplay
    {
        get
        {
            if (DeletedItem.ExpirationDate == null)
                return "Never";

            var daysLeft = (DeletedItem.ExpirationDate.Value - DateTime.UtcNow).TotalDays;
            return daysLeft > 0
                ? $"in {(int)daysLeft} days"
                : "Expired";
        }
    }
}
