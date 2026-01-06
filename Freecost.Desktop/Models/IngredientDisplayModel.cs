using Freecost.Core.Helpers;
using Freecost.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Desktop.Models;

/// <summary>
/// Display wrapper for Ingredient to make it DataGrid-friendly
/// Includes validation status for costing
/// </summary>
public class IngredientDisplayModel
{
    private readonly Ingredient _ingredient;

    public IngredientDisplayModel(Ingredient ingredient)
    {
        _ingredient = ingredient;
    }

    public Ingredient Ingredient => _ingredient;

    // Main identifier
    public string Name => _ingredient.Name;

    // Alias for recipes
    public string Alias
    {
        get
        {
            // Only show first alias if any exist - no fallback extraction
            if (_ingredient.Aliases != null && _ingredient.Aliases.Any())
            {
                return _ingredient.Aliases.First().AliasName;
            }

            return "";
        }
    }

    // Vendor information
    public string Supplier => _ingredient.VendorName ?? "";
    public string SKU => _ingredient.VendorSku ?? "";

    // Pricing (CurrentPrice = price per unit)
    public string Price => _ingredient.CurrentPrice.ToString("C2");

    // For now, show "1" as quantity since we're storing unit price
    public string Quantity => "1.00";

    // Unit abbreviation
    public string Unit => UnitConverter.GetAbbreviation(_ingredient.Unit);

    // Category for filtering
    public string Category => _ingredient.Category ?? "";

    // ========================================
    // NEW: Status validation for costing
    // ========================================

    /// <summary>
    /// Visual status indicator: ✅ if OK, ⚠️ if has issues
    /// </summary>
    public string StatusIcon
    {
        get
        {
            var issues = GetValidationIssues();
            return issues.Count == 0 ? "✅" : "⚠️";
        }
    }

    /// <summary>
    /// Tooltip text explaining any issues
    /// </summary>
    public string StatusTooltip
    {
        get
        {
            var issues = GetValidationIssues();
            if (issues.Count == 0)
                return "Ready to use in recipes";

            return "Issues:\n" + string.Join("\n", issues.Select(i => $"• {i}"));
        }
    }

    /// <summary>
    /// Color for the status icon
    /// </summary>
    public string StatusColor => GetValidationIssues().Count == 0 ? "#4CAF50" : "#FF9800";

    /// <summary>
    /// Validates the ingredient and returns list of issues
    /// </summary>
    private List<string> GetValidationIssues()
    {
        var issues = new List<string>();

        // Check 1: Price must be greater than 0
        if (_ingredient.CurrentPrice <= 0)
        {
            issues.Add("Price must be greater than $0.00");
        }

        // Check 2: If using alternate unit, conversion must be complete
        if (_ingredient.UseAlternateUnit)
        {
            if (!_ingredient.AlternateUnit.HasValue)
            {
                issues.Add("Alternate unit is not selected");
            }

            if (!_ingredient.AlternateConversionQuantity.HasValue || _ingredient.AlternateConversionQuantity.Value <= 0)
            {
                issues.Add("Conversion quantity must be greater than 0");
            }

            if (!_ingredient.AlternateConversionUnit.HasValue)
            {
                issues.Add("Conversion unit is not selected");
            }

            // Check 3: If all conversion fields are set, verify the conversion is valid
            if (_ingredient.AlternateUnit.HasValue &&
                _ingredient.AlternateConversionUnit.HasValue &&
                !UnitConverter.CanConvert(_ingredient.AlternateConversionUnit.Value, _ingredient.Unit))
            {
                issues.Add($"Cannot convert {_ingredient.AlternateConversionUnit.Value} to {_ingredient.Unit}");
            }
        }

        return issues;
    }

    /// <summary>
    /// True if ingredient can be used for costing (no validation issues)
    /// </summary>
    public bool IsValid => GetValidationIssues().Count == 0;
}