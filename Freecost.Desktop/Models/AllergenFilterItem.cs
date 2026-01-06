using CommunityToolkit.Mvvm.ComponentModel;
using Freecost.Core.Enums;

namespace Freecost.Desktop.Models;

public partial class AllergenFilterItem : ObservableObject
{
    public AllergenType Type { get; set; }
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
