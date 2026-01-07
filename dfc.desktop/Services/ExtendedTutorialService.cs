using System.Collections.Generic;
using System.Linq;
using TutorialModule = Dfc.Desktop.Models.TutorialModule;
using TutorialStep = Dfc.Desktop.Models.TutorialStep;
using TutorialAnnotation = Dfc.Desktop.Models.TutorialAnnotation;
using TutorialAnnotationType = Dfc.Desktop.Models.TutorialAnnotationType;

namespace Dfc.Desktop.Services;

/// <summary>
/// Service that provides the complete extended tutorial content
/// </summary>
public interface IExtendedTutorialService
{
    List<Dfc.Desktop.Models.TutorialModule> GetAllModules();
    Dfc.Desktop.Models.TutorialModule? GetModuleById(string moduleId);
    Dfc.Desktop.Models.TutorialStep? GetStepById(string moduleId, string stepId);
    int GetTotalStepCount();
    (Dfc.Desktop.Models.TutorialModule?, Dfc.Desktop.Models.TutorialStep?) GetNextStep(string currentModuleId, string currentStepId);
    (Dfc.Desktop.Models.TutorialModule?, Dfc.Desktop.Models.TutorialStep?) GetPreviousStep(string currentModuleId, string currentStepId);
}

public class ExtendedTutorialService : IExtendedTutorialService
{
    private readonly List<Dfc.Desktop.Models.TutorialModule> _modules;

    public ExtendedTutorialService()
    {
        _modules = BuildAllTutorialModules();
    }

    public List<Dfc.Desktop.Models.TutorialModule> GetAllModules() => _modules;

    public Dfc.Desktop.Models.TutorialModule? GetModuleById(string moduleId) =>
        _modules.FirstOrDefault(m => m.Id == moduleId);

    public Dfc.Desktop.Models.TutorialStep? GetStepById(string moduleId, string stepId) =>
        GetModuleById(moduleId)?.Steps.FirstOrDefault(s => s.Id == stepId);

    public int GetTotalStepCount() =>
        _modules.Sum(m => m.Steps.Count);

    public (Dfc.Desktop.Models.TutorialModule?, Dfc.Desktop.Models.TutorialStep?) GetNextStep(string currentModuleId, string currentStepId)
    {
        var currentModule = GetModuleById(currentModuleId);
        if (currentModule == null) return (null, null);

        var currentStepIndex = currentModule.Steps.FindIndex(s => s.Id == currentStepId);
        if (currentStepIndex == -1) return (null, null);

        // Check if there's a next step in the current module
        if (currentStepIndex < currentModule.Steps.Count - 1)
        {
            return (currentModule, currentModule.Steps[currentStepIndex + 1]);
        }

        // Move to the next module
        var currentModuleIndex = _modules.FindIndex(m => m.Id == currentModuleId);
        if (currentModuleIndex < _modules.Count - 1)
        {
            var nextModule = _modules[currentModuleIndex + 1];
            return (nextModule, nextModule.Steps.FirstOrDefault());
        }

        return (null, null); // End of tutorial
    }

    public (Dfc.Desktop.Models.TutorialModule?, Dfc.Desktop.Models.TutorialStep?) GetPreviousStep(string currentModuleId, string currentStepId)
    {
        var currentModule = GetModuleById(currentModuleId);
        if (currentModule == null) return (null, null);

        var currentStepIndex = currentModule.Steps.FindIndex(s => s.Id == currentStepId);
        if (currentStepIndex == -1) return (null, null);

        // Check if there's a previous step in the current module
        if (currentStepIndex > 0)
        {
            return (currentModule, currentModule.Steps[currentStepIndex - 1]);
        }

        // Move to the previous module
        var currentModuleIndex = _modules.FindIndex(m => m.Id == currentModuleId);
        if (currentModuleIndex > 0)
        {
            var previousModule = _modules[currentModuleIndex - 1];
            return (previousModule, previousModule.Steps.LastOrDefault());
        }

        return (null, null); // Beginning of tutorial
    }

    private List<Dfc.Desktop.Models.TutorialModule> BuildAllTutorialModules()
    {
        return new List<Dfc.Desktop.Models.TutorialModule>
        {
            BuildGettingStartedModule(),
            BuildIngredientModule(),
            BuildRecipeModule(),
            BuildEntreeModule(),
            BuildDashboardModule(),
            BuildAdvancedModule()
        };
    }

    #region Module 1: Getting Started

    private Dfc.Desktop.Models.TutorialModule BuildGettingStartedModule()
    {
        return new Dfc.Desktop.Models.TutorialModule
        {
            Id = "getting-started",
            Name = "Getting Started",
            Description = "Learn the basics of navigating Desktop Food Cost",
            Icon = "🚀",
            EstimatedMinutes = 5,
            DisplayOrder = 1,
            Steps = new List<Dfc.Desktop.Models.TutorialStep>
            {
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "welcome",
                    Title = "Welcome to Desktop Food Cost! 👋",
                    Description = "Desktop Food Cost helps restaurant owners and food service professionals manage recipes, calculate costs, and track profitability. This interactive tutorial will walk you through all the key features.\n\nYou can pause and resume this tutorial anytime from Settings → Help & Learning.",
                    StepNumber = 1,
                    ProTip = "Press F1 anytime to access help and keyboard shortcuts!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "navigation",
                    Title = "Main Navigation",
                    Description = "The left sidebar is your main navigation panel. Click any icon to switch between different sections:\n\n• Dashboard - Overview and analytics\n• Ingredients - Manage your inventory\n• Recipes - Create and cost recipes\n• Entrees - Build menu items\n• Menu Plan - Plan your weekly menu\n• Settings - Configure the app",
                    ScreenshotPath = "main-navigation.png",
                    KeyboardShortcut = "Ctrl+1 through Ctrl+6",
                    StepNumber = 2,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 2, Y = 15, Width = 12, Height = 60, Animate = true },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Arrow, X = 15, Y = 20, Direction = 270, Length = 8, Text = "Quick navigation" }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "global-search",
                    Title = "Global Search - Find Anything Fast",
                    Description = "Press Ctrl+K from anywhere to open Global Search. This powerful feature lets you instantly find:\n\n• Any ingredient by name or category\n• Any recipe you've created\n• Any menu item\n\nStart typing to see results in real-time. Press Enter to jump directly to that item.",
                    ScreenshotPath = "global-search.png",
                    KeyboardShortcut = "Ctrl+K",
                    StepNumber = 3,
                    ProTip = "Global Search is the fastest way to navigate when you have hundreds of items!",
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.CircleHighlight, X = 95, Y = 5, Width = 4, Height = 4, Animate = true },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Callout, X = 85, Y = 8, Text = "Or click here" }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "keyboard-shortcuts",
                    Title = "Keyboard Shortcuts",
                    Description = "Desktop Food Cost is built for speed with powerful keyboard shortcuts:\n\n• Ctrl+1 to Ctrl+6 - Navigate to any section\n• Ctrl+K - Global Search\n• Ctrl+F - Search current view\n• Ctrl+N - Add new item (context-aware)\n• F1 - Help and shortcuts\n• Ctrl+P - Print current item\n• Esc - Close dialogs\n\nThese shortcuts work from anywhere in the app!",
                    ScreenshotPath = "keyboard-shortcuts.png",
                    KeyboardShortcut = "Press F1 for full list",
                    StepNumber = 4,
                    ProTip = "Master these shortcuts to work 3x faster!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "status-notifications",
                    Title = "Status Bar & Notifications",
                    Description = "The status bar at the bottom shows important notifications:\n\n• Green notifications - Success messages\n• Blue notifications - Information\n• Yellow notifications - Warnings\n• Red notifications - Errors\n\nNotifications automatically disappear after a few seconds, except errors which stay until you dismiss them.",
                    ScreenshotPath = "status-bar.png",
                    StepNumber = 5,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 0, Y = 95, Width = 100, Height = 5, Animate = true }
                    }
                }
            }
        };
    }

    #endregion

    #region Module 2: Managing Ingredients

    private Dfc.Desktop.Models.TutorialModule BuildIngredientModule()
    {
        return new Dfc.Desktop.Models.TutorialModule
        {
            Id = "ingredients",
            Name = "Managing Ingredients",
            Description = "Learn how to add and organize your ingredients",
            Icon = "📦",
            EstimatedMinutes = 8,
            DisplayOrder = 2,
            Steps = new List<Dfc.Desktop.Models.TutorialStep>
            {
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "ingredients-overview",
                    Title = "Ingredients - Your Foundation",
                    Description = "Ingredients are the building blocks of everything in Desktop Food Cost. Before you can create recipes or menu items, you'll need to add your ingredients.\n\nEach ingredient tracks:\n• Name and category\n• Current price and case quantity\n• Vendor information\n• Price history over time\n• Allergen information\n• Multiple names (aliases)",
                    NavigationTarget = "Ingredients",
                    StepNumber = 1
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "add-ingredient",
                    Title = "Adding Your First Ingredient",
                    Description = "Click the 'Add Ingredient' button (or press Ctrl+N) to open the ingredient dialog.\n\nRequired fields:\n• Name - What you call this ingredient\n• Price - Cost per case/package\n• Case Quantity - How much is in a case\n• Unit - Pounds, gallons, each, etc.\n\nOptional but recommended:\n• Category - For organization\n• Vendor - Where you buy it\n• SKU - Vendor's product code",
                    ScreenshotPath = "add-ingredient-dialog.png",
                    KeyboardShortcut = "Ctrl+N",
                    StepNumber = 2,
                    ProTip = "Use specific names like 'All-Purpose Flour' instead of just 'Flour' for clarity!",
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 20, Y = 25, Text = "1" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 20, Y = 35, Text = "2" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 20, Y = 45, Text = "3" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 20, Y = 55, Text = "4" }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "ingredient-pricing",
                    Title = "Understanding Ingredient Pricing",
                    Description = "Desktop Food Cost calculates cost per unit automatically:\n\nExample: If you pay $50 for a 50lb bag of flour:\n• Current Price: $50\n• Case Quantity: 50\n• Unit: Pounds\n• Cost per pound: $1.00 (calculated automatically)\n\nWhen you use 2 pounds in a recipe, Desktop Food Cost knows that costs $2.00.\n\nThis system works for any unit: pounds, gallons, ounces, each, etc.",
                    ScreenshotPath = "ingredient-pricing.png",
                    StepNumber = 3,
                    ProTip = "Always enter prices as you actually purchase them - Desktop Food Cost handles the math!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "categories",
                    Title = "Categories & Organization",
                    Description = "Organize ingredients with categories:\n\n• Meat & Poultry\n• Seafood\n• Dairy & Eggs\n• Fresh Produce\n• Dry Goods\n• Spices & Seasonings\n• Beverages\n...and more!\n\nCategories get automatic color coding for quick visual identification. You can filter and search by category to find items faster.",
                    ScreenshotPath = "ingredient-categories.png",
                    StepNumber = 4,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 45, Y = 12, Width = 15, Height = 8 },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Label, X = 62, Y = 14, Text = "Color-coded!" }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "bulk-import",
                    Title = "Bulk Import from Spreadsheets",
                    Description = "Have a lot of ingredients? Use Bulk Import to save hours of data entry!\n\nClick 'Bulk Import' to upload:\n• Vendor Excel files\n• CSV exports\n• Custom spreadsheets\n\nDesktop Food Cost will automatically map columns like:\n• Product Name → Name\n• Case Price → Current Price\n• Pack Size → Case Quantity\n• UOM → Unit\n\nYou can review and edit everything before importing.",
                    ScreenshotPath = "bulk-import.png",
                    StepNumber = 5,
                    ProTip = "Most restaurant suppliers provide Excel price sheets - import them directly!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "search-filter",
                    Title = "Search & Filter Ingredients",
                    Description = "Finding ingredients is fast and easy:\n\n1. Search box (Ctrl+F) - Type any part of the name, category, or vendor\n2. Category filter - Show only items from specific categories\n3. Sort by columns - Click any column header\n\nSearch works across:\n• Ingredient names\n• Categories\n• Vendor names\n• SKU codes\n• Aliases (alternative names)",
                    ScreenshotPath = "search-ingredients.png",
                    KeyboardShortcut = "Ctrl+F",
                    StepNumber = 6,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Arrow, X = 50, Y = 5, Direction = 180, Length = 15, Text = "Search here" }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "price-history",
                    Title = "Price History Tracking",
                    Description = "Desktop Food Cost automatically tracks every price change:\n\n• Click any ingredient to see its price history\n• View a chart showing price trends over time\n• See exact dates and amounts for each change\n• Calculate average cost over any period\n\nThis helps you:\n• Negotiate with vendors\n• Plan for seasonal price changes\n• Spot unusual price increases\n• Make data-driven purchasing decisions",
                    ScreenshotPath = "price-history.png",
                    StepNumber = 7,
                    ProTip = "Review price history quarterly to identify cost-saving opportunities!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "batch-operations",
                    Title = "Batch Operations",
                    Description = "Select multiple ingredients to perform bulk actions:\n\n1. Click checkboxes to select items\n2. Use the batch toolbar that appears\n3. Available operations:\n   • Delete selected items\n   • Duplicate items\n   • Change category\n   • Update vendor\n   • Export to Excel\n\nThis is perfect for:\n• Reorganizing categories\n• Deleting seasonal items\n• Updating vendor info",
                    ScreenshotPath = "batch-operations.png",
                    StepNumber = 8,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 2, Y = 15, Width = 5, Height = 25, Animate = true },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 0, Y = 8, Width = 100, Height = 5, Color = "#7AB51D" }
                    }
                }
            }
        };
    }

    #endregion

    #region Module 3: Creating Recipes

    private Dfc.Desktop.Models.TutorialModule BuildRecipeModule()
    {
        return new Dfc.Desktop.Models.TutorialModule
        {
            Id = "recipes",
            Name = "Creating Recipes",
            Description = "Build recipes and calculate costs automatically",
            Icon = "🍳",
            EstimatedMinutes = 7,
            DisplayOrder = 3,
            Steps = new List<Dfc.Desktop.Models.TutorialStep>
            {
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "recipes-overview",
                    Title = "Recipes - The Heart of Your Kitchen",
                    Description = "Recipes are where individual ingredients come together. Each recipe in Desktop Food Cost includes:\n\n• List of ingredients with quantities\n• Preparation instructions\n• Yield (how many servings it makes)\n• Automatic cost calculation\n• Cost per serving\n• Prep time and difficulty\n• Categories and tags\n• Allergen detection\n\nRecipes can be used to build menu items (entrees) or as components of other recipes.",
                    NavigationTarget = "Recipes",
                    StepNumber = 1
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "create-recipe",
                    Title = "Creating Your First Recipe",
                    Description = "Click 'Add Recipe' (or press Ctrl+N) to start:\n\n1. Enter basic info:\n   • Recipe name\n   • Category (appetizer, main, side, etc.)\n   • Yield and yield unit (4 servings, 1 pan, etc.)\n\n2. Add ingredients:\n   • Search for ingredients\n   • Enter quantities\n   • Choose units\n\n3. Write instructions\n4. Save and see the calculated cost!",
                    ScreenshotPath = "create-recipe.png",
                    KeyboardShortcut = "Ctrl+N",
                    StepNumber = 2,
                    ProTip = "Start with your most popular or profitable items first!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "add-ingredients-to-recipe",
                    Title = "Adding Ingredients to Recipes",
                    Description = "For each ingredient in your recipe:\n\n1. Click 'Add Ingredient'\n2. Search by name (start typing)\n3. Enter the quantity you need\n4. Select the unit\n\nDesktop Food Cost automatically calculates the cost for each ingredient based on:\n• The ingredient's current price\n• The quantity you're using\n• Unit conversions (it knows 16oz = 1lb!)\n\nThe total recipe cost updates in real-time as you add ingredients.",
                    ScreenshotPath = "recipe-ingredients.png",
                    StepNumber = 3,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Arrow, X = 85, Y = 30, Direction = 0, Length = 10, Text = "Add more" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 15, Y = 75, Width = 30, Height = 8, Animate = true, Color = "#FFB74D" }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "yield-servings",
                    Title = "Yield & Servings",
                    Description = "The yield tells Desktop Food Cost how much the recipe makes:\n\nExamples:\n• Yield: 4, Unit: servings (makes 4 servings)\n• Yield: 1, Unit: pan (makes 1 full pan)\n• Yield: 12, Unit: cookies (makes 12 cookies)\n\nThis is crucial because Desktop Food Cost divides the total recipe cost by the yield to calculate cost per serving.\n\nExample:\n• Total recipe cost: $12.00\n• Yield: 4 servings\n• Cost per serving: $3.00",
                    ScreenshotPath = "recipe-yield.png",
                    StepNumber = 4,
                    ProTip = "Use 'servings' for most recipes - it makes menu pricing easier!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "recipe-instructions",
                    Title = "Instructions & Documentation",
                    Description = "Add preparation instructions to standardize your recipes:\n\n• Step-by-step preparation\n• Cooking temperatures and times\n• Plating instructions\n• Special techniques or tips\n\nGood documentation helps:\n• Train new kitchen staff\n• Maintain consistency\n• Reduce errors and waste\n• Preserve institutional knowledge\n\nYou can print recipe cards with instructions and ingredient lists for your kitchen.",
                    ScreenshotPath = "recipe-instructions.png",
                    KeyboardShortcut = "Ctrl+P to print",
                    StepNumber = 5
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "recipe-categories",
                    Title = "Recipe Categories & Tags",
                    Description = "Organize recipes with categories and tags:\n\nCategories (one per recipe):\n• Appetizers\n• Main Course\n• Sides\n• Desserts\n• Sauces\n• Prep/Components\n\nTags (multiple per recipe):\n• vegetarian, vegan, gluten-free\n• quick, make-ahead\n• seasonal, signature\n• Any custom tags you want!\n\nSearch and filter by category or tag to find recipes quickly.",
                    ScreenshotPath = "recipe-categories.png",
                    StepNumber = 6,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 15, Y = 12, Width = 30, Height = 6 },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 15, Y = 20, Width = 60, Height = 6 }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "allergen-detection",
                    Title = "Automatic Allergen Detection",
                    Description = "Desktop Food Cost automatically detects allergens in your recipes!\n\nWhen you add ingredients, Desktop Food Cost checks for:\n• Milk & dairy\n• Eggs\n• Fish & shellfish\n• Tree nuts & peanuts\n• Wheat & gluten\n• Soy\n• Sesame\n\nAllergens are shown with clear icons. You can:\n• View all allergens for any recipe\n• Filter recipes by allergen\n• Print allergen information for menus\n• Track dietary restrictions",
                    ScreenshotPath = "recipe-allergens.png",
                    StepNumber = 7,
                    ProTip = "This helps you comply with allergen labeling requirements!"
                }
            }
        };
    }

    #endregion

    #region Module 4: Building Entrees

    private Dfc.Desktop.Models.TutorialModule BuildEntreeModule()
    {
        return new Dfc.Desktop.Models.TutorialModule
        {
            Id = "entrees",
            Name = "Building Menu Items",
            Description = "Create entrees and analyze profitability",
            Icon = "🍽️",
            EstimatedMinutes = 6,
            DisplayOrder = 4,
            Steps = new List<Dfc.Desktop.Models.TutorialStep>
            {
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "entrees-overview",
                    Title = "Entrees - Your Menu Items",
                    Description = "Entrees represent the actual items on your menu that customers order. They're built from one or more recipes.\n\nExamples:\n• 'Steak Dinner' might include:\n  - 1 serving Grilled Steak recipe\n  - 1 serving Mashed Potatoes recipe\n  - 1 serving Green Beans recipe\n\nFor each entree, Desktop Food Cost calculates:\n• Total food cost\n• Food cost percentage\n• Gross profit\n• Profit margin",
                    NavigationTarget = "Entrees",
                    StepNumber = 1
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "create-entree",
                    Title = "Creating a Menu Item",
                    Description = "Click 'Add Entree' (or press Ctrl+N):\n\n1. Enter entree information:\n   • Menu item name\n   • Description (what customers see)\n   • Category\n   • Menu price (what you charge)\n\n2. Add recipes:\n   • Search for recipes\n   • Set quantity (servings per plate)\n\n3. Desktop Food Cost automatically calculates:\n   • Total food cost\n   • Food cost percentage\n   • Profit per plate",
                    ScreenshotPath = "create-entree.png",
                    KeyboardShortcut = "Ctrl+N",
                    StepNumber = 2,
                    ProTip = "Add garnishes and sides as simple recipes to track their costs too!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "recipe-components",
                    Title = "Adding Recipe Components",
                    Description = "Build your entree by adding recipe components:\n\n1. Click 'Add Recipe'\n2. Search for the recipe\n3. Enter quantity (usually 1 serving)\n4. Repeat for all components\n\nExample - 'Pasta Primavera' might include:\n• 1 serving Pasta recipe\n• 0.5 serving Marinara Sauce recipe\n• 1 serving Grilled Vegetables recipe\n• 0.25 serving Parmesan Garnish recipe\n\nDesktop Food Cost calculates the total cost by summing all components.",
                    ScreenshotPath = "entree-components.png",
                    StepNumber = 3,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 5, Y = 30, Text = "1" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 5, Y = 38, Text = "2" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 5, Y = 46, Text = "3" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 5, Y = 54, Text = "4" }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "menu-pricing",
                    Title = "Setting Menu Prices",
                    Description = "Enter the price you charge customers for this menu item.\n\nDesktop Food Cost shows you:\n\nFood Cost Percentage = (Food Cost ÷ Menu Price) × 100\n\nExample:\n• Food cost: $6.00\n• Menu price: $18.00\n• Food cost %: 33.3%\n\nIndustry standards:\n🟢 Below 30% - Excellent\n🟡 30-40% - Good\n🔴 Above 40% - Consider repricing\n\nAdjust your menu price or portions to hit your target food cost percentage.",
                    ScreenshotPath = "menu-pricing.png",
                    StepNumber = 4,
                    ProTip = "Most successful restaurants target 25-35% food cost!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "profitability-analysis",
                    Title = "Understanding Profitability",
                    Description = "Desktop Food Cost shows complete profitability metrics:\n\n• Food Cost - What it costs to make\n• Menu Price - What you charge\n• Food Cost % - (Cost ÷ Price) × 100\n• Gross Profit - Price - Cost\n• Profit Margin % - (Profit ÷ Price) × 100\n\nTraffic light indicators:\n🟢 Green - Highly profitable\n🟡 Yellow - Acceptable\n🔴 Red - Needs attention\n\nUse this data to:\n• Adjust menu prices\n• Modify portions\n• Negotiate better ingredient prices\n• Remove unprofitable items",
                    ScreenshotPath = "profitability-metrics.png",
                    StepNumber = 5,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 70, Y = 25, Width = 25, Height = 40, Animate = true }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "menu-photos",
                    Title = "Photos & Presentation",
                    Description = "Add photos to your menu items:\n\n1. Click 'Add Photo'\n2. Select an image file\n3. The photo is stored with your entree\n\nBenefits:\n• Visual menu boards\n• Staff training\n• Marketing materials\n• Online ordering\n• Social media\n\nPhotos help with:\n• Plating consistency\n• Training new servers\n• Appealing menu design\n• Customer expectations",
                    ScreenshotPath = "entree-photos.png",
                    StepNumber = 6,
                    ProTip = "Take photos of your best plating to maintain consistency!"
                }
            }
        };
    }

    #endregion

    #region Module 5: Dashboard Analytics

    private Dfc.Desktop.Models.TutorialModule BuildDashboardModule()
    {
        return new Dfc.Desktop.Models.TutorialModule
        {
            Id = "dashboard",
            Name = "Dashboard & Analytics",
            Description = "Monitor costs and analyze profitability",
            Icon = "📊",
            EstimatedMinutes = 5,
            DisplayOrder = 5,
            Steps = new List<Dfc.Desktop.Models.TutorialStep>
            {
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "dashboard-overview",
                    Title = "Your Command Center",
                    Description = "The Dashboard gives you a complete overview of your restaurant's costs and profitability at a glance.\n\nKey metrics:\n• Total ingredients tracked\n• Total recipes created\n• Total menu items\n• Average food cost %\n• Most expensive ingredients\n• Cost trends over time\n\nUse the Dashboard daily to:\n• Spot cost increases\n• Track profitability trends\n• Identify problem areas\n• Make data-driven decisions",
                    NavigationTarget = "Dashboard",
                    KeyboardShortcut = "Ctrl+1",
                    StepNumber = 1
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "cost-metrics",
                    Title = "Understanding Cost Metrics",
                    Description = "Key dashboard metrics explained:\n\nTotal Ingredients:\n• Number of ingredients in your database\n• Indicates inventory complexity\n\nTotal Recipes:\n• Number of standardized recipes\n• Shows menu development progress\n\nAverage Food Cost %:\n• Average across all menu items\n• Target: 25-35% for most restaurants\n\nTotal Inventory Value:\n• Sum of all ingredient costs\n• Helps track purchasing trends",
                    ScreenshotPath = "dashboard-metrics.png",
                    StepNumber = 2,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 15, Y = 20, Text = "1" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 40, Y = 20, Text = "2" },
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.NumberBadge, X = 65, Y = 20, Text = "3" }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "charts-trends",
                    Title = "Charts & Visual Analytics",
                    Description = "The Dashboard includes visual charts:\n\nPrice Trend Chart:\n• Shows ingredient price changes over time\n• Spot seasonal patterns\n• Identify unusual price spikes\n\nCategory Breakdown:\n• Pie chart of spending by category\n• See where most money goes\n• Identify cost-saving opportunities\n\nFood Cost Distribution:\n• Bar chart showing entree profitability\n• Quick view of winners and losers\n• Guide menu engineering decisions",
                    ScreenshotPath = "dashboard-charts.png",
                    StepNumber = 3,
                    ProTip = "Review charts weekly to catch trends early!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "top-ingredients",
                    Title = "Most Expensive Ingredients",
                    Description = "The 'Top Ingredients by Cost' section shows:\n\n• Your most expensive ingredients\n• Total spent per ingredient\n• Percentage of total costs\n\nUse this to:\n• Negotiate better prices on high-cost items\n• Find substitutes for expensive ingredients\n• Track usage of premium items\n• Spot waste opportunities\n\nExample: If chicken breast is 25% of your costs, a 10% price reduction saves you 2.5% overall!",
                    ScreenshotPath = "top-ingredients.png",
                    StepNumber = 4,
                    Annotations = new List<Dfc.Desktop.Models.TutorialAnnotation>
                    {
                        new Dfc.Desktop.Models.TutorialAnnotation { Type = Dfc.Desktop.Models.TutorialAnnotationType.Highlight, X = 10, Y = 50, Width = 80, Height = 35 }
                    }
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "export-reports",
                    Title = "Exporting Reports",
                    Description = "Export data for external analysis:\n\nFrom Settings → Export Data:\n• Export all ingredients to Excel\n• Export all recipes to Excel\n• Export all menu items to Excel\n\nExported files include:\n• All costs and prices\n• Categories and details\n• Calculated metrics\n• Formatted for easy reading\n\nUse exports for:\n• Sharing with partners or accountants\n• Custom analysis in Excel\n• Backup documentation\n• Board presentations",
                    ScreenshotPath = "export-data.png",
                    StepNumber = 5,
                    ProTip = "Export monthly for historical records!"
                }
            }
        };
    }

    #endregion

    #region Module 6: Advanced Features

    private Dfc.Desktop.Models.TutorialModule BuildAdvancedModule()
    {
        return new Dfc.Desktop.Models.TutorialModule
        {
            Id = "advanced",
            Name = "Advanced Features",
            Description = "Cloud sync, backups, and power features",
            Icon = "⚡",
            EstimatedMinutes = 10,
            DisplayOrder = 6,
            Steps = new List<Dfc.Desktop.Models.TutorialStep>
            {
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "advanced-overview",
                    Title = "Power User Features",
                    Description = "Desktop Food Cost includes advanced features for power users:\n\n• Cloud Sync - Access from multiple devices\n• Automatic Backups - Never lose data\n• Recycle Bin - Recover deleted items\n• Multiple Locations - Manage multiple restaurants\n• Shopping Lists - Auto-generate from recipes\n• Menu Planning - Plan weekly menus\n\nThese features help you:\n• Work more efficiently\n• Protect your data\n• Scale your operation\n• Collaborate with team members",
                    StepNumber = 1
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "data-management",
                    Title = "Data Management",
                    Description = "Desktop Food Cost stores your data locally:\n\n• All your data is stored on your computer\n• Use the Backup feature to protect your data\n• Export to Excel for sharing\n• Import from invoices and spreadsheets\n\nBenefits:\n• Fast and reliable\n• Works offline\n• Your data stays private\n• No subscription required",
                    ScreenshotPath = "data-management.png",
                    StepNumber = 2,
                    ProTip = "Regular backups protect your data!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "database-backup",
                    Title = "Database Backup & Restore",
                    Description = "Protect your data with backups:\n\nFrom Settings → Database Management:\n\n1. Backup Database\n   • Creates a complete backup file\n   • Includes all ingredients, recipes, entrees\n   • Save to external drive for safety\n\n2. Restore Database\n   • Restore from any previous backup\n   • Replaces current data\n   • Useful for disaster recovery\n\n3. Backup Manager\n   • Automatic scheduled backups\n   • View backup history\n   • Restore from any backup point",
                    ScreenshotPath = "backup-restore.png",
                    StepNumber = 3,
                    ProTip = "Set up automatic backups and save copies to cloud storage!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "recycle-bin",
                    Title = "Recycle Bin - Undo Deletions",
                    Description = "Deleted something by mistake? Use the Recycle Bin!\n\nWhen you delete items, they move to the Recycle Bin:\n• Ingredients\n• Recipes\n• Menu items\n\nRecycle Bin features:\n• View all deleted items\n• Restore items with one click\n• Permanently delete old items\n• Search within deleted items\n• Auto-purge after 30 days\n\nAccess from Settings → Recycle Bin",
                    ScreenshotPath = "recycle-bin.png",
                    StepNumber = 4,
                    ProTip = "Items stay in Recycle Bin for 30 days before permanent deletion"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "unit-conversions",
                    Title = "Universal Unit Conversion System",
                    Description = "Desktop Food Cost automatically converts between different units!\n\nExample: Recipe calls for '2 cups flour' but you buy flour by the pound?\n• Desktop Food Cost converts automatically\n• No manual math needed\n• Works with 50+ unit types\n\n3-Layer Conversion System:\n1. Ingredient-Specific: Custom conversions you define\n2. USDA Database: Auto-extracted from nutritional data\n3. Standard Conversions: Built-in weight/volume conversions\n\nThis means recipes cost correctly regardless of unit mismatches!",
                    ScreenshotPath = "unit-conversions.png",
                    StepNumber = 5,
                    ProTip = "USDA conversions are automatically extracted when you save ingredients!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "usda-integration",
                    Title = "USDA Nutritional Data Integration",
                    Description = "Desktop Food Cost integrates with the USDA FoodData Central database:\n\nWhen adding ingredients:\n• Search USDA database for matches\n• Auto-populate nutritional information\n• Extract serving size conversions\n• Detect allergens automatically\n\nExample: 'Chicken Breast'\n• USDA data: 1 medium = 227g\n• Desktop Food Cost saves this conversion\n• Now you can use 'each' or 'grams' interchangeably\n\nThis builds a comprehensive conversion database automatically!",
                    ScreenshotPath = "usda-integration.png",
                    StepNumber = 6,
                    ProTip = "The more ingredients you add, the smarter your conversion database becomes!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "conversion-manager",
                    Title = "Managing Unit Conversions",
                    Description = "View and manage all your conversions in Settings:\n\nGo to Settings → Unit Conversions to see:\n• Total conversions available\n• USDA-sourced conversions\n• Your custom conversions\n• Coverage percentage (% of ingredients with conversions)\n\nYou can:\n• View all conversions with source badges\n• See which came from USDA vs manual entry\n• Refresh statistics anytime\n• Add custom conversions in ingredient details\n\nThe Conversions tab in Add/Edit Ingredient shows all conversions for that specific ingredient.",
                    ScreenshotPath = "conversion-manager.png",
                    StepNumber = 7,
                    ProTip = "Check Settings → Unit Conversions to see your conversion coverage!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "shopping-lists",
                    Title = "Auto-Generated Shopping Lists",
                    Description = "Create shopping lists from your menu:\n\n1. Go to Menu Plan\n2. Add entrees for the week\n3. Click 'Generate Shopping List'\n4. Desktop Food Cost calculates:\n   • All ingredients needed\n   • Quantities for each\n   • Organized by category\n   • Vendor information\n\n5. Print or export the list\n\nThis saves hours of manual calculation and reduces ordering errors!",
                    ScreenshotPath = "shopping-list.png",
                    StepNumber = 8,
                    ProTip = "Use shopping lists to order exactly what you need - reduce waste!"
                },
                new Dfc.Desktop.Models.TutorialStep
                {
                    Id = "congratulations",
                    Title = "Congratulations! 🎉",
                    Description = "You've completed the full Desktop Food Cost tutorial!\n\nYou now know how to:\n✅ Navigate efficiently with keyboard shortcuts\n✅ Manage ingredients and track prices\n✅ Create recipes and calculate costs\n✅ Build menu items and analyze profitability\n✅ Use the Dashboard for insights\n✅ Leverage advanced features\n\nWhat's next?\n• Start adding your real ingredients\n• Create your signature recipes\n• Build your menu items\n• Track your costs and profitability\n\nRemember: You can always access this tutorial from Settings → Help & Learning.\n\nHappy cooking! 👨‍🍳",
                    StepNumber = 9
                }
            }
        };
    }

    #endregion
}
