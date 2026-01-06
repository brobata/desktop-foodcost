// Location: Dfc.Desktop/ViewModels/MenuPlanViewModel.cs
// Action: CREATE - Menu planning feature for organizing entrees by date/event

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class MenuPlanViewModel : ViewModelBase
{
    private readonly IEntreeService _entreeService;
    private readonly ILogger<MenuPlanViewModel>? _logger;
    private readonly Guid _currentLocationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public MenuPlanViewModel(IEntreeService entreeService, ILogger<MenuPlanViewModel>? logger = null)
    {
        _entreeService = entreeService;
        _logger = logger;
        MenuPlans = new ObservableCollection<MenuPlanModel>();
        AvailableEntrees = new ObservableCollection<EntreeItemModel>();
    }

    [ObservableProperty]
    private ObservableCollection<MenuPlanModel> _menuPlans;

    [ObservableProperty]
    private ObservableCollection<EntreeItemModel> _availableEntrees;

    [ObservableProperty]
    private MenuPlanModel? _selectedMenuPlan;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private int _totalPlans;

    [ObservableProperty]
    private decimal _totalMonthCost;

    public async Task LoadAsync()
    {
        try
        {
            IsLoading = true;

            // Load available entrees
            var entrees = await _entreeService.GetAllEntreesAsync(_currentLocationId);
            AvailableEntrees.Clear();
            foreach (var entree in entrees.OrderBy(e => e.Name))
            {
                AvailableEntrees.Add(new EntreeItemModel
                {
                    Id = entree.Id,
                    Name = entree.Name,
                    Cost = entree.TotalCost,
                    Category = entree.Category ?? "Uncategorized"
                });
            }

            // Create sample menu plans for the current month
            LoadSampleMenuPlans();

            TotalPlans = MenuPlans.Count;
            TotalMonthCost = MenuPlans.Sum(m => m.TotalCost);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading menu plans");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadSampleMenuPlans()
    {
        // Generate menu plans for the next 7 days
        var today = DateTime.Today;
        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(i);
            MenuPlans.Add(new MenuPlanModel
            {
                Date = date,
                Name = date.DayOfWeek.ToString() + " Menu",
                EventType = i == 5 || i == 6 ? "Weekend Special" : "Regular Service",
                ExpectedGuests = i == 5 || i == 6 ? 120 : 80,
                Entrees = new ObservableCollection<MenuPlanEntreeModel>()
            });
        }
    }

    [RelayCommand]
    private void AddMenuPlan()
    {
        var newPlan = new MenuPlanModel
        {
            Date = DateTime.Today,
            Name = "New Menu Plan",
            EventType = "Regular Service",
            ExpectedGuests = 50,
            Entrees = new ObservableCollection<MenuPlanEntreeModel>()
        };
        MenuPlans.Add(newPlan);
        SelectedMenuPlan = newPlan;
        TotalPlans = MenuPlans.Count;
    }

    [RelayCommand]
    private void DeleteMenuPlan(MenuPlanModel? menuPlan)
    {
        if (menuPlan != null)
        {
            MenuPlans.Remove(menuPlan);
            TotalPlans = MenuPlans.Count;
            TotalMonthCost = MenuPlans.Sum(m => m.TotalCost);
        }
    }

    [RelayCommand]
    private void AddEntreeToMenu(EntreeItemModel? entree)
    {
        if (entree == null || SelectedMenuPlan == null) return;

        var menuEntree = new MenuPlanEntreeModel
        {
            EntreeId = entree.Id,
            EntreeName = entree.Name,
            UnitCost = entree.Cost,
            Quantity = 1,
            TotalCost = entree.Cost
        };

        SelectedMenuPlan.Entrees.Add(menuEntree);
        SelectedMenuPlan.RecalculateTotalCost();
        TotalMonthCost = MenuPlans.Sum(m => m.TotalCost);
    }

    [RelayCommand]
    private void RemoveEntreeFromMenu(MenuPlanEntreeModel? entree)
    {
        if (entree == null || SelectedMenuPlan == null) return;

        SelectedMenuPlan.Entrees.Remove(entree);
        SelectedMenuPlan.RecalculateTotalCost();
        TotalMonthCost = MenuPlans.Sum(m => m.TotalCost);
    }
}

public partial class MenuPlanModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _eventType = string.Empty;

    [ObservableProperty]
    private int _expectedGuests;

    [ObservableProperty]
    private ObservableCollection<MenuPlanEntreeModel> _entrees = new();

    [ObservableProperty]
    private decimal _totalCost;

    [ObservableProperty]
    private decimal _costPerGuest;

    public string DateDisplay => Date.ToString("ddd, MMM dd, yyyy");

    public void RecalculateTotalCost()
    {
        TotalCost = Entrees.Sum(e => e.TotalCost);
        CostPerGuest = ExpectedGuests > 0 ? TotalCost / ExpectedGuests : 0;
    }
}

public partial class MenuPlanEntreeModel : ObservableObject
{
    [ObservableProperty]
    private Guid _entreeId;

    [ObservableProperty]
    private string _entreeName = string.Empty;

    [ObservableProperty]
    private decimal _unitCost;

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private decimal _totalCost;

    partial void OnQuantityChanged(int value)
    {
        TotalCost = value * UnitCost;
    }
}

public partial class EntreeItemModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _cost;

    [ObservableProperty]
    private string _category = string.Empty;
}
