using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class CustomReportService : ICustomReportService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IEntreeRepository _entreeRepository;
    private readonly IRecipeCostCalculator _costCalculator;

    public CustomReportService(
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        IEntreeRepository entreeRepository,
        IRecipeCostCalculator costCalculator)
    {
        _ingredientRepository = ingredientRepository;
        _recipeRepository = recipeRepository;
        _entreeRepository = entreeRepository;
        _costCalculator = costCalculator;
    }

    public async Task<CustomReport> GenerateReportAsync(ReportDefinition definition, Guid locationId)
    {
        return definition.Type switch
        {
            ReportType.Ingredients => await GenerateIngredientReportAsync(definition, locationId),
            ReportType.Recipes => await GenerateRecipeReportAsync(definition, locationId),
            ReportType.Entrees => await GenerateEntreeReportAsync(definition, locationId),
            ReportType.CostAnalysis => await GenerateCostAnalysisReportAsync(definition, locationId),
            _ => new CustomReport { Name = definition.Name, Type = definition.Type }
        };
    }

    public async Task<List<ReportTemplate>> GetAvailableTemplatesAsync()
    {
        return await Task.FromResult(new List<ReportTemplate>
        {
            new ReportTemplate
            {
                Id = "ingredient-cost-summary",
                Name = "Ingredient Cost Summary",
                Description = "Overview of all ingredients with current prices and vendors",
                Type = ReportType.Ingredients,
                DefaultMetrics = new List<string> { "Name", "Category", "CurrentPrice", "VendorName", "Unit" },
                DefaultGroupBy = "Category",
                DefaultSortBy = "CurrentPrice"
            },
            new ReportTemplate
            {
                Id = "recipe-cost-analysis",
                Name = "Recipe Cost Analysis",
                Description = "Cost breakdown of all recipes",
                Type = ReportType.Recipes,
                DefaultMetrics = new List<string> { "Name", "Category", "TotalCost", "CostPerServing", "Yield" },
                DefaultGroupBy = "Category",
                DefaultSortBy = "TotalCost"
            },
            new ReportTemplate
            {
                Id = "high-cost-ingredients",
                Name = "High Cost Ingredients",
                Description = "Top 20 most expensive ingredients",
                Type = ReportType.Ingredients,
                DefaultMetrics = new List<string> { "Name", "CurrentPrice", "VendorName", "Category" },
                DefaultGroupBy = "",
                DefaultSortBy = "CurrentPrice"
            },
            new ReportTemplate
            {
                Id = "entree-profitability",
                Name = "Entree Profitability",
                Description = "Profit margins for all menu entrees",
                Type = ReportType.Entrees,
                DefaultMetrics = new List<string> { "Name", "MenuPrice", "TotalCost", "FoodCostPercentage" },
                DefaultGroupBy = "Category",
                DefaultSortBy = "FoodCostPercentage"
            },
            new ReportTemplate
            {
                Id = "vendor-spend-analysis",
                Name = "Vendor Spend Analysis",
                Description = "Total spending by vendor",
                Type = ReportType.VendorComparison,
                DefaultMetrics = new List<string> { "VendorName", "IngredientCount", "TotalValue" },
                DefaultGroupBy = "VendorName",
                DefaultSortBy = "TotalValue"
            }
        });
    }

    public ReportDefinition CreateFromTemplate(ReportTemplate template)
    {
        return new ReportDefinition
        {
            Name = template.Name,
            Type = template.Type,
            SelectedMetrics = template.DefaultMetrics,
            GroupBy = template.DefaultGroupBy,
            SortBy = template.DefaultSortBy,
            SortDescending = true
        };
    }

    private async Task<CustomReport> GenerateIngredientReportAsync(ReportDefinition definition, Guid locationId)
    {
        var ingredients = await _ingredientRepository.GetAllAsync(locationId);
        var filteredIngredients = ApplyFilters(ingredients, definition.Filters);

        var rows = filteredIngredients.Select(i => new Dictionary<string, object>
        {
            ["Name"] = i.Name,
            ["Category"] = i.Category ?? "Uncategorized",
            ["CurrentPrice"] = i.CurrentPrice,
            ["Unit"] = i.Unit.ToString(),
            ["VendorName"] = i.VendorName ?? "Unknown",
            ["CaseQuantity"] = i.CaseQuantity
        }).ToList();

        // Apply sorting
        if (!string.IsNullOrEmpty(definition.SortBy))
        {
            rows = definition.SortDescending
                ? rows.OrderByDescending(r => r.GetValueOrDefault(definition.SortBy, 0)).ToList()
                : rows.OrderBy(r => r.GetValueOrDefault(definition.SortBy, 0)).ToList();
        }

        var report = new CustomReport
        {
            Name = definition.Name,
            Type = definition.Type,
            ColumnHeaders = definition.SelectedMetrics.Any()
                ? definition.SelectedMetrics
                : new List<string> { "Name", "Category", "CurrentPrice", "Unit", "VendorName" },
            Rows = rows,
            Summary = new Dictionary<string, decimal>
            {
                ["TotalIngredients"] = ingredients.Count(),
                ["AveragePrice"] = ingredients.Any() ? ingredients.Average(i => i.CurrentPrice) : 0,
                ["TotalValue"] = ingredients.Sum(i => i.CurrentPrice)
            }
        };

        return report;
    }

    private async Task<CustomReport> GenerateRecipeReportAsync(ReportDefinition definition, Guid locationId)
    {
        var recipes = (await _recipeRepository.GetAllRecipesAsync(locationId)).ToList();

        // Calculate costs
        foreach (var recipe in recipes)
        {
            await _costCalculator.CalculateRecipeTotalCostAsync(recipe);
        }

        var rows = recipes.Select(r => new Dictionary<string, object>
        {
            ["Name"] = r.Name,
            ["Category"] = r.Category ?? "Uncategorized",
            ["TotalCost"] = r.TotalCost,
            ["CostPerServing"] = r.CostPerServing,
            ["Yield"] = r.Yield,
            ["YieldUnit"] = r.YieldUnit,
            ["PrepTimeMinutes"] = r.PrepTimeMinutes ?? 0,
            ["Difficulty"] = r.Difficulty.ToString()
        }).ToList();

        // Apply sorting
        if (!string.IsNullOrEmpty(definition.SortBy))
        {
            rows = definition.SortDescending
                ? rows.OrderByDescending(r => r.GetValueOrDefault(definition.SortBy, 0)).ToList()
                : rows.OrderBy(r => r.GetValueOrDefault(definition.SortBy, 0)).ToList();
        }

        var report = new CustomReport
        {
            Name = definition.Name,
            Type = definition.Type,
            ColumnHeaders = definition.SelectedMetrics.Any()
                ? definition.SelectedMetrics
                : new List<string> { "Name", "Category", "TotalCost", "CostPerServing", "Yield" },
            Rows = rows,
            Summary = new Dictionary<string, decimal>
            {
                ["TotalRecipes"] = recipes.Count,
                ["AverageCost"] = recipes.Any() ? recipes.Average(r => r.TotalCost) : 0,
                ["TotalCostAllRecipes"] = recipes.Sum(r => r.TotalCost)
            }
        };

        return report;
    }

    private async Task<CustomReport> GenerateEntreeReportAsync(ReportDefinition definition, Guid locationId)
    {
        var entrees = await _entreeRepository.GetAllAsync(locationId);

        var rows = entrees.Select(e => new Dictionary<string, object>
        {
            ["Name"] = e.Name,
            ["Category"] = e.Category ?? "Uncategorized",
            ["MenuPrice"] = e.MenuPrice ?? 0,
            ["TotalCost"] = e.TotalCost,
            ["FoodCostPercentage"] = e.FoodCostPercentage
        }).ToList();

        // Apply sorting
        if (!string.IsNullOrEmpty(definition.SortBy))
        {
            rows = definition.SortDescending
                ? rows.OrderByDescending(r => r.GetValueOrDefault(definition.SortBy, 0)).ToList()
                : rows.OrderBy(r => r.GetValueOrDefault(definition.SortBy, 0)).ToList();
        }

        var report = new CustomReport
        {
            Name = definition.Name,
            Type = definition.Type,
            ColumnHeaders = definition.SelectedMetrics.Any()
                ? definition.SelectedMetrics
                : new List<string> { "Name", "Category", "MenuPrice", "TotalCost", "FoodCostPercentage" },
            Rows = rows,
            Summary = new Dictionary<string, decimal>
            {
                ["TotalEntrees"] = entrees.Count(),
                ["AverageFoodCostPercent"] = entrees.Any() ? entrees.Average(e => e.FoodCostPercentage) : 0
            }
        };

        return report;
    }

    private async Task<CustomReport> GenerateCostAnalysisReportAsync(ReportDefinition definition, Guid locationId)
    {
        var ingredients = await _ingredientRepository.GetAllAsync(locationId);

        // Group by category
        var categoryGroups = ingredients
            .GroupBy(i => i.Category ?? "Uncategorized")
            .Select(g => new Dictionary<string, object>
            {
                ["Category"] = g.Key,
                ["IngredientCount"] = g.Count(),
                ["TotalValue"] = g.Sum(i => i.CurrentPrice),
                ["AveragePrice"] = g.Average(i => i.CurrentPrice)
            })
            .ToList();

        var report = new CustomReport
        {
            Name = definition.Name,
            Type = definition.Type,
            ColumnHeaders = new List<string> { "Category", "IngredientCount", "TotalValue", "AveragePrice" },
            Rows = categoryGroups.OrderByDescending(cg => (decimal)cg["TotalValue"]).ToList(),
            Summary = new Dictionary<string, decimal>
            {
                ["TotalCategories"] = categoryGroups.Count,
                ["TotalValue"] = ingredients.Sum(i => i.CurrentPrice)
            }
        };

        return report;
    }

    private List<Ingredient> ApplyFilters(IEnumerable<Ingredient> ingredients, List<ReportFilter> filters)
    {
        var filtered = ingredients.AsEnumerable();

        foreach (var filter in filters)
        {
            filtered = filter.Field.ToLower() switch
            {
                "category" => ApplyCategoryFilter(filtered, filter),
                "vendorname" => ApplyVendorFilter(filtered, filter),
                _ => filtered
            };
        }

        return filtered.ToList();
    }

    private IEnumerable<Ingredient> ApplyCategoryFilter(IEnumerable<Ingredient> ingredients, ReportFilter filter)
    {
        return filter.Operator switch
        {
            FilterOperator.Equals => ingredients.Where(i => i.Category == filter.Value),
            FilterOperator.Contains => ingredients.Where(i => i.Category?.Contains(filter.Value, StringComparison.OrdinalIgnoreCase) == true),
            _ => ingredients
        };
    }

    private IEnumerable<Ingredient> ApplyVendorFilter(IEnumerable<Ingredient> ingredients, ReportFilter filter)
    {
        return filter.Operator switch
        {
            FilterOperator.Equals => ingredients.Where(i => i.VendorName == filter.Value),
            FilterOperator.Contains => ingredients.Where(i => i.VendorName?.Contains(filter.Value, StringComparison.OrdinalIgnoreCase) == true),
            _ => ingredients
        };
    }
}
