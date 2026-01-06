using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace Freecost.Desktop.ViewModels;

public partial class SubstitutionsGuideViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SubstitutionGroup> _filteredSubstitutions = new();

    private readonly ObservableCollection<SubstitutionGroup> _allSubstitutions = new();

    public SubstitutionsGuideViewModel()
    {
        LoadSubstitutions();
        FilteredSubstitutions = _allSubstitutions;
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredSubstitutions = new ObservableCollection<SubstitutionGroup>(_allSubstitutions);
        }
        else
        {
            var filtered = _allSubstitutions.Where(s =>
                s.OriginalIngredient.Contains(value, System.StringComparison.OrdinalIgnoreCase) ||
                s.Substitutes.Any(sub => sub.Substitute.Contains(value, System.StringComparison.OrdinalIgnoreCase))
            ).ToList();

            FilteredSubstitutions = new ObservableCollection<SubstitutionGroup>(filtered);
        }
    }

    private void LoadSubstitutions()
    {
        // Dairy Substitutions
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Butter",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Margarine", Ratio = "1:1" },
                new() { Substitute = "1 cup Vegetable Oil", Ratio = "3/4 cup" },
                new() { Substitute = "1 cup Coconut Oil", Ratio = "1:1" },
                new() { Substitute = "1 cup Applesauce (for baking)", Ratio = "1:1" }
            },
            Notes = "Use unsalted butter unless recipe specifies salted. Reduce oil by 25% in baking recipes."
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Whole Milk",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Almond Milk", Ratio = "1:1" },
                new() { Substitute = "1 cup Soy Milk", Ratio = "1:1" },
                new() { Substitute = "1 cup Oat Milk", Ratio = "1:1" },
                new() { Substitute = "1/2 cup Evaporated Milk + 1/2 cup Water", Ratio = "1:1" },
                new() { Substitute = "1 cup Water + 1/4 cup Dry Milk Powder", Ratio = "1:1" }
            }
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Heavy Cream",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "3/4 cup Milk + 1/4 cup Melted Butter", Ratio = "1:1" },
                new() { Substitute = "1 cup Coconut Cream", Ratio = "1:1" },
                new() { Substitute = "1 cup Evaporated Milk", Ratio = "1:1" }
            }
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Sour Cream",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Greek Yogurt", Ratio = "1:1" },
                new() { Substitute = "1 cup Cottage Cheese (blended)", Ratio = "1:1" },
                new() { Substitute = "7/8 cup Buttermilk + 3 Tbsp Butter", Ratio = "1:1" }
            }
        });

        // Egg Substitutions
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 Egg",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 Tbsp Ground Flaxseed + 3 Tbsp Water", Ratio = "1:1" },
                new() { Substitute = "1 Tbsp Chia Seeds + 3 Tbsp Water", Ratio = "1:1" },
                new() { Substitute = "1/4 cup Applesauce", Ratio = "1:1" },
                new() { Substitute = "1/4 cup Mashed Banana", Ratio = "1:1" },
                new() { Substitute = "1/4 cup Silken Tofu", Ratio = "1:1" },
                new() { Substitute = "3 Tbsp Aquafaba (chickpea water)", Ratio = "1:1" }
            },
            Notes = "Let flax/chia mixtures sit 5 minutes before using. Best for binding in baking."
        });

        // Sugar Substitutions
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Granulated Sugar",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Brown Sugar (packed)", Ratio = "1:1" },
                new() { Substitute = "3/4 cup Honey (reduce liquid by 1/4 cup)", Ratio = "3/4 cup" },
                new() { Substitute = "3/4 cup Maple Syrup (reduce liquid by 3 Tbsp)", Ratio = "3/4 cup" },
                new() { Substitute = "2/3 cup Agave Nectar (reduce liquid)", Ratio = "2/3 cup" },
                new() { Substitute = "1 1/2 cups Powdered Sugar", Ratio = "1.5:1" }
            },
            Notes = "When using liquid sweeteners, reduce other liquids in recipe and lower oven temp by 25°F."
        });

        // Flour Substitutions
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup All-Purpose Flour",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Bread Flour", Ratio = "1:1" },
                new() { Substitute = "1 cup + 2 Tbsp Cake Flour", Ratio = "1.1:1" },
                new() { Substitute = "1/2 cup Whole Wheat + 1/2 cup All-Purpose", Ratio = "1:1" },
                new() { Substitute = "7/8 cup Almond Flour (for gluten-free)", Ratio = "7/8 cup" },
                new() { Substitute = "1 cup Gluten-Free Flour Blend", Ratio = "1:1" }
            },
            Notes = "Bread flour has more protein. Cake flour is finer. GF flours may need xanthan gum."
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 Tbsp Cornstarch (thickening)",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "2 Tbsp All-Purpose Flour", Ratio = "2:1" },
                new() { Substitute = "1 Tbsp Arrowroot Powder", Ratio = "1:1" },
                new() { Substitute = "1 Tbsp Tapioca Starch", Ratio = "1:1" },
                new() { Substitute = "2 Tbsp Quick-Cooking Tapioca", Ratio = "2:1" }
            }
        });

        // Baking Agents
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 tsp Baking Powder",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1/4 tsp Baking Soda + 1/2 tsp Cream of Tartar", Ratio = "1:1" },
                new() { Substitute = "1/4 tsp Baking Soda + 1/2 cup Buttermilk", Ratio = "1:1" }
            },
            Notes = "Must reduce other acidic liquids if using buttermilk substitution."
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Buttermilk",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Milk + 1 Tbsp Lemon Juice or Vinegar", Ratio = "1:1" },
                new() { Substitute = "1 cup Plain Yogurt", Ratio = "1:1" },
                new() { Substitute = "1 cup Milk + 1 3/4 tsp Cream of Tartar", Ratio = "1:1" }
            },
            Notes = "Let milk + acid mixture sit 5-10 minutes before using."
        });

        // Chocolate Substitutions
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 oz Unsweetened Chocolate",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "3 Tbsp Cocoa Powder + 1 Tbsp Butter/Oil", Ratio = "1:1" }
            }
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 oz Semi-Sweet Chocolate",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 oz Unsweetened Chocolate + 1 Tbsp Sugar", Ratio = "1:1" },
                new() { Substitute = "3 Tbsp Cocoa + 1 Tbsp Fat + 1 Tbsp Sugar", Ratio = "1:1" }
            }
        });

        // Herbs & Spices
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 Tbsp Fresh Herbs",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 tsp Dried Herbs", Ratio = "1/3" }
            },
            Notes = "Fresh herbs are milder than dried. Use 1/3 the amount of dried."
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 clove Fresh Garlic",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1/8 tsp Garlic Powder", Ratio = "1/8 tsp" },
                new() { Substitute = "1/2 tsp Garlic Salt (reduce salt)", Ratio = "1/2 tsp" },
                new() { Substitute = "1/2 tsp Granulated Garlic", Ratio = "1/2 tsp" }
            }
        });

        // Vinegar & Acids
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 Tbsp Lemon Juice",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 Tbsp Lime Juice", Ratio = "1:1" },
                new() { Substitute = "1 Tbsp White Vinegar", Ratio = "1:1" },
                new() { Substitute = "1 Tbsp White Wine", Ratio = "1:1" }
            }
        });

        // Cooking Wine & Alcohol
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Red Wine",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Grape Juice + 2 Tbsp Vinegar", Ratio = "1:1" },
                new() { Substitute = "1 cup Beef Broth + 2 Tbsp Red Wine Vinegar", Ratio = "1:1" },
                new() { Substitute = "1 cup Cranberry Juice", Ratio = "1:1" }
            }
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup White Wine",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup White Grape Juice + 2 Tbsp Vinegar", Ratio = "1:1" },
                new() { Substitute = "1 cup Chicken/Vegetable Broth", Ratio = "1:1" },
                new() { Substitute = "1 cup Apple Juice + 2 Tbsp Lemon Juice", Ratio = "1:1" }
            }
        });

        // Breadcrumbs
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Breadcrumbs",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Crushed Crackers", Ratio = "1:1" },
                new() { Substitute = "1 cup Crushed Cornflakes", Ratio = "1:1" },
                new() { Substitute = "1 cup Panko", Ratio = "1:1" },
                new() { Substitute = "3/4 cup Ground Oats", Ratio = "3/4 cup" }
            }
        });

        // Tomato Products
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Tomato Sauce",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1/2 cup Tomato Paste + 1/2 cup Water", Ratio = "1:1" },
                new() { Substitute = "3/4 cup Tomato Puree + 1/4 cup Water", Ratio = "1:1" }
            }
        });

        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Tomato Paste",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Tomato Sauce (simmer to reduce)", Ratio = "reduce to 1/2" }
            },
            Notes = "Simmer tomato sauce until reduced by half for paste consistency."
        });

        // Nuts
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 cup Walnuts",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 cup Pecans", Ratio = "1:1" },
                new() { Substitute = "1 cup Almonds", Ratio = "1:1" },
                new() { Substitute = "1 cup Hazelnuts", Ratio = "1:1" }
            },
            Notes = "Most nuts can be substituted 1:1, though flavor will vary."
        });

        // Mustard
        _allSubstitutions.Add(new SubstitutionGroup
        {
            OriginalIngredient = "1 Tbsp Dijon Mustard",
            Substitutes = new ObservableCollection<SubstituteItem>
            {
                new() { Substitute = "1 Tbsp Yellow Mustard", Ratio = "1:1" },
                new() { Substitute = "1 Tbsp Whole Grain Mustard", Ratio = "1:1" },
                new() { Substitute = "1 tsp Dry Mustard + 2 tsp Water", Ratio = "1:1" }
            }
        });
    }
}

public partial class SubstitutionGroup : ObservableObject
{
    [ObservableProperty]
    private string _originalIngredient = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SubstituteItem> _substitutes = new();

    [ObservableProperty]
    private string? _notes;
}

public partial class SubstituteItem : ObservableObject
{
    [ObservableProperty]
    private string _substitute = string.Empty;

    [ObservableProperty]
    private string _ratio = string.Empty;
}
