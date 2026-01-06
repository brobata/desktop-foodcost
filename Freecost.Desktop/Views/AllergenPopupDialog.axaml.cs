using Avalonia.Controls;
using Freecost.Core.Enums;
using Freecost.Core.Services;
using Freecost.Desktop.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Desktop.Views;

public partial class AllergenPopupDialog : Window
{
    public bool WasSaved { get; private set; }
    public List<AllergenItemViewModel> SelectedAllergens { get; private set; } = new();

    public AllergenPopupDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initialize the popup with auto-detected allergens from an entree or recipe
    /// </summary>
    public void SetAutoDetectedAllergens(Dictionary<AllergenType, List<string>> detectedAllergens)
    {
        AllergenSelector.SetAutoDetectedAllergens(detectedAllergens);
    }

    /// <summary>
    /// Initialize the popup with previously selected allergens (when editing)
    /// </summary>
    public void SetExistingAllergens(List<AllergenItemViewModel> existingAllergens)
    {
        foreach (var existing in existingAllergens)
        {
            var allergen = AllergenSelector.Allergens.FirstOrDefault(a => a.AllergenType == existing.AllergenType);
            if (allergen != null)
            {
                allergen.IsSelected = existing.IsSelected;
                allergen.IsAutoDetected = existing.IsAutoDetected;
                allergen.SourceIngredients = existing.SourceIngredients;
            }
        }
    }

    private void OnSave(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WasSaved = true;
        SelectedAllergens = AllergenSelector.GetSelectedAllergens();
        Close();
    }

    private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WasSaved = false;
        Close();
    }
}
