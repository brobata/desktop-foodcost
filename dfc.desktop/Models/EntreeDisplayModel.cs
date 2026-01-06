using Dfc.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.Desktop.Models;

public class EntreeDisplayModel
{
    private readonly Entree _entree;

    public EntreeDisplayModel(Entree entree)
    {
        _entree = entree;
    }

    public Entree Entree => _entree;

    public string Name => _entree.Name;
    public string Category => "Menu Item";
    public string MenuPrice => _entree.MenuPrice?.ToString("C2") ?? "$0.00";
    public string FoodCost => _entree.TotalCost.ToString("C2");
    public string FoodCostPercent
    {
        get
        {
            if (_entree.MenuPrice.HasValue && _entree.MenuPrice.Value > 0 && _entree.TotalCost > 0)
            {
                return $"{((_entree.TotalCost / _entree.MenuPrice.Value) * 100):F1}%";
            }
            return "0.0%";
        }
    }

    // Profitability Indicators (Traffic Light)
    public decimal FoodCostPercentValue
    {
        get
        {
            if (_entree.MenuPrice.HasValue && _entree.MenuPrice.Value > 0 && _entree.TotalCost > 0)
            {
                return (_entree.TotalCost / _entree.MenuPrice.Value) * 100;
            }
            return 0;
        }
    }

    public string ProfitabilityIcon
    {
        get
        {
            var foodCostPct = FoodCostPercentValue;
            if (foodCostPct == 0) return "⚪"; // No data
            if (foodCostPct <= 30) return "🟢"; // Excellent profitability
            if (foodCostPct <= 40) return "🟡"; // Good profitability
            return "🔴"; // Poor profitability
        }
    }

    public string ProfitabilityColor
    {
        get
        {
            var foodCostPct = FoodCostPercentValue;
            if (foodCostPct == 0) return "#9E9E9E"; // Gray - no data
            if (foodCostPct <= 30) return "#4CAF50"; // Green
            if (foodCostPct <= 40) return "#FF9800"; // Amber/Yellow
            return "#EF5350"; // Red
        }
    }

    public string ProfitabilityTooltip
    {
        get
        {
            var foodCostPct = FoodCostPercentValue;
            if (foodCostPct == 0) return "No pricing data";

            var profitMargin = 100 - foodCostPct;
            if (foodCostPct <= 30) return $"Excellent: {profitMargin:F1}% profit margin";
            if (foodCostPct <= 40) return $"Good: {profitMargin:F1}% profit margin";
            return $"Poor: {profitMargin:F1}% profit margin (high food cost)";
        }
    }

    public int ComponentCount
    {
        get
        {
            var recipes = _entree.EntreeRecipes?.Count ?? 0;
            var ingredients = _entree.EntreeIngredients?.Count ?? 0;
            return recipes + ingredients;
        }
    }

    public string StatusIcon
    {
        get
        {
            var issues = GetValidationIssues();
            return issues.Count == 0 ? "✅" : "⚠️";
        }
    }

    public string StatusTooltip
    {
        get
        {
            var issues = GetValidationIssues();
            if (issues.Count == 0)
                return "Entree is properly configured";

            return "Issues:\n" + string.Join("\n", issues.Select(i => $"• {i}"));
        }
    }

    public string StatusColor => GetValidationIssues().Count == 0 ? "#4CAF50" : "#FF9800";

    private List<string> GetValidationIssues()
    {
        var issues = new List<string>();

        if (!_entree.MenuPrice.HasValue || _entree.MenuPrice.Value <= 0)
        {
            issues.Add("Menu price is missing or $0.00");
        }

        var hasRecipes = _entree.EntreeRecipes != null && _entree.EntreeRecipes.Any();
        var hasIngredients = _entree.EntreeIngredients != null && _entree.EntreeIngredients.Any();

        if (!hasRecipes && !hasIngredients)
        {
            issues.Add("Entree has no components");
        }

        if (_entree.MenuPrice.HasValue && _entree.MenuPrice.Value > 0 && _entree.TotalCost > 0)
        {
            var foodCostPercent = (_entree.TotalCost / _entree.MenuPrice.Value) * 100;
            if (foodCostPercent > 40)
            {
                issues.Add($"High food cost: {foodCostPercent:F1}% (target <40%)");
            }
        }

        return issues;
    }
}