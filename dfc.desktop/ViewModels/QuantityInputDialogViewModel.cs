using CommunityToolkit.Mvvm.ComponentModel;
using Dfc.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.Desktop.ViewModels;

public partial class QuantityInputDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _quantity = "1";

    [ObservableProperty]
    private UnitType _selectedUnit;

    public List<UnitType> Units { get; }

    public QuantityInputDialogViewModel(string title, UnitType defaultUnit = UnitType.Each)
    {
        _title = title;
        _selectedUnit = defaultUnit;

        Units = Enum.GetValues<UnitType>().ToList();
    }

    public bool IsValid()
    {
        return decimal.TryParse(Quantity, out var qty) && qty > 0;  // Changed from _quantity
    }

    public decimal GetQuantity()
    {
        return decimal.TryParse(Quantity, out var qty) ? qty : 0;  // Changed from _quantity
    }
}