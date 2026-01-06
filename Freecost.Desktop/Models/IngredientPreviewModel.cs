using Freecost.Core.Enums;
using Freecost.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Desktop.Models;

/// <summary>
/// Wrapper for displaying ingredient import preview with case price, total quantity, and validation status
/// </summary>
public class IngredientPreviewModel
{
    public Ingredient Ingredient { get; }
    public decimal CasePrice { get; }
    public decimal TotalQuantity { get; }

    // Validation properties
    public bool HasErrors { get; set; }
    public bool HasWarnings { get; set; }
    public string ValidationErrors { get; set; } = string.Empty;
    public string ValidationWarnings { get; set; } = string.Empty;

    public IngredientPreviewModel(Ingredient ingredient, decimal casePrice, decimal totalQuantity)
    {
        Ingredient = ingredient;
        CasePrice = casePrice;
        TotalQuantity = totalQuantity;
    }

    // Display properties for DataGrid
    public string Name => Ingredient.Name;
    public string DisplayPrice => CasePrice > 0 ? $"${CasePrice:F2}" : "Missing Price";
    public string DisplayQuantity => $"{TotalQuantity:F0} {GetUnitDisplay(Ingredient.Unit)}";
    public string VendorName => Ingredient.VendorName ?? "";
    public string VendorSku => Ingredient.VendorSku ?? "";
    public string Category => Ingredient.Category ?? "";

    // Validation status display
    public string StatusIcon => HasErrors ? "❌" : HasWarnings ? "⚠️" : "✓";
    public string StatusText => HasErrors ? "Error" : HasWarnings ? "Warning" : "Valid";
    public string StatusColor => HasErrors ? "#FFEBEE" : HasWarnings ? "#FFF3E0" : "#E8F5E9";
    public string StatusForeground => HasErrors ? "#C62828" : HasWarnings ? "#E65100" : "#2E7D32";

    // Tooltip combining errors and warnings
    public string ValidationTooltip
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(ValidationErrors))
                parts.Add($"ERRORS: {ValidationErrors}");
            if (!string.IsNullOrEmpty(ValidationWarnings))
                parts.Add($"WARNINGS: {ValidationWarnings}");
            return parts.Any() ? string.Join("\n\n", parts) : "Valid - no issues detected";
        }
    }

    private static string GetUnitDisplay(UnitType unit)
    {
        return unit == UnitType.Count ? "CT" : unit.ToString();
    }
}
