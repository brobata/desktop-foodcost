namespace Dfc.Core.Models;

/// <summary>
/// User preferences for print customization
/// </summary>
public class PrintSettings
{
    // Page settings
    public PageSize PageSize { get; set; } = PageSize.Letter;
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
    public double MarginTop { get; set; } = 1.0;
    public double MarginBottom { get; set; } = 1.0;
    public double MarginLeft { get; set; } = 1.0;
    public double MarginRight { get; set; } = 1.0;

    // Header/Footer
    public bool ShowHeader { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
    public bool ShowPageNumbers { get; set; } = true;
    public bool ShowGeneratedDate { get; set; } = true;
    public bool ShowCompanyLogo { get; set; } = false;
    public string? CompanyLogoPath { get; set; }
    public string? CompanyName { get; set; }

    // Typography
    public string FontFamily { get; set; } = "Arial";
    public int BaseFontSize { get; set; } = 10;
    public int HeaderFontSize { get; set; } = 20;
    public int SubHeaderFontSize { get; set; } = 14;

    // Colors
    public string HeaderColor { get; set; } = "#1565C0"; // Blue.Darken3
    public string AccentColor { get; set; } = "#1976D2"; // Blue.Darken2
    public string TextColor { get; set; } = "#000000";

    // Content visibility for Recipe cards
    public bool ShowRecipePhoto { get; set; } = true;
    public bool ShowIngredients { get; set; } = true;
    public bool ShowInstructions { get; set; } = true;
    public bool ShowNutritionInfo { get; set; } = true;
    public bool ShowCostBreakdown { get; set; } = true;
    public bool ShowPricePerServing { get; set; } = true;
    public bool ShowYieldInfo { get; set; } = true;
    public bool ShowPrepTime { get; set; } = false;
    public bool ShowCookTime { get; set; } = false;

    // Content visibility for Entree cards
    public bool ShowEntreePhoto { get; set; } = true;
    public bool ShowRecipeComponents { get; set; } = true;
    public bool ShowEntreeCost { get; set; } = true;
    public bool ShowProfitMargin { get; set; } = true;
    public bool ShowSellingPrice { get; set; } = true;

    // Content visibility for Ingredient lists
    public bool GroupByCategory { get; set; } = true;
    public bool ShowVendorInfo { get; set; } = true;
    public bool ShowUnitInfo { get; set; } = true;
    public bool ShowPriceHistory { get; set; } = false;
    public bool ShowAllergenInfo { get; set; } = false;

    // Layout options
    public bool UseColoredHeaders { get; set; } = true;
    public bool UseTableBorders { get; set; } = true;
    public bool AlternateRowColors { get; set; } = true;

    // Export options
    public bool AutoOpenAfterGeneration { get; set; } = true;
    public string DefaultSaveLocation { get; set; } = "";
}

public enum PageSize
{
    Letter,
    Legal,
    A4,
    A5
}

public enum PageOrientation
{
    Portrait,
    Landscape
}
