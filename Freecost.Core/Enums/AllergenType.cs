namespace Freecost.Core.Enums;

public enum AllergenType
{
    // FDA Big 9
    Milk,
    Eggs,
    Fish,
    Shellfish,
    TreeNuts,
    Peanuts,
    Wheat,
    Soybeans,
    Sesame,

    // Dietary
    Vegan,
    Vegetarian,
    Pescatarian,
    GlutenFree,

    // Religious
    Kosher,
    Halal,

    // Additional
    ContainsAlcohol,
    Nightshades,
    Sulfites,
    AddedSugar
}