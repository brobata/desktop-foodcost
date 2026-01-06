// Location: Dfc.Core/Enums/CategoryType.cs
// Action: CREATE NEW FILE

namespace Dfc.Core.Enums;

/// <summary>
/// Standard ingredient categories for organization and filtering
/// </summary>
public enum CategoryType
{
    MeatAndPoultry = 0,
    SeafoodAndFish = 1,
    DairyAndEggs = 2,
    FreshProduce = 3,
    DryGoods = 4,
    DrySpicesAndHerbs = 5,
    PaperAndDisposables = 6,
    FrozenItems = 7,
    BakeryItems = 8,
    NonAlcoholicBeverage = 9,
    Beer = 10,
    Liquor = 11,
    Wine = 12
}

/// <summary>
/// Extension methods for CategoryType enum
/// </summary>
public static class CategoryTypeExtensions
{
    /// <summary>
    /// Get display name for category
    /// </summary>
    public static string GetDisplayName(this CategoryType category)
    {
        return category switch
        {
            CategoryType.MeatAndPoultry => "Meat & Poultry",
            CategoryType.SeafoodAndFish => "Seafood & Fish",
            CategoryType.DairyAndEggs => "Dairy & Eggs",
            CategoryType.FreshProduce => "Fresh Produce",
            CategoryType.DryGoods => "Dry Goods",
            CategoryType.DrySpicesAndHerbs => "Dry Spices & Herbs",
            CategoryType.PaperAndDisposables => "Paper & Disposables",
            CategoryType.FrozenItems => "Frozen Items",
            CategoryType.BakeryItems => "Bakery Items",
            CategoryType.NonAlcoholicBeverage => "Non-Alcoholic Beverage",
            CategoryType.Beer => "Beer",
            CategoryType.Liquor => "Liquor",
            CategoryType.Wine => "Wine",
            _ => category.ToString()
        };
    }

    /// <summary>
    /// Parse display name back to enum
    /// </summary>
    public static CategoryType? FromDisplayName(string displayName)
    {
        return displayName switch
        {
            "Meat & Poultry" => CategoryType.MeatAndPoultry,
            "Seafood & Fish" => CategoryType.SeafoodAndFish,
            "Dairy & Eggs" => CategoryType.DairyAndEggs,
            "Fresh Produce" => CategoryType.FreshProduce,
            "Dry Goods" => CategoryType.DryGoods,
            "Dry Spices & Herbs" => CategoryType.DrySpicesAndHerbs,
            "Paper & Disposables" => CategoryType.PaperAndDisposables,
            "Frozen Items" => CategoryType.FrozenItems,
            "Bakery Items" => CategoryType.BakeryItems,
            "Non-Alcoholic Beverage" => CategoryType.NonAlcoholicBeverage,
            "Beer" => CategoryType.Beer,
            "Liquor" => CategoryType.Liquor,
            "Wine" => CategoryType.Wine,
            _ => null
        };
    }
}