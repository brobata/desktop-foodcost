using Dfc.Core.Models;

namespace Dfc.Core.Services;

/// <summary>
/// Service for importing recipes from URLs (AllRecipes, Food Network, etc.)
/// </summary>
public interface IRecipeUrlImportService
{
    /// <summary>
    /// Imports a recipe from a URL by scraping the web page
    /// </summary>
    Task<Recipe?> ImportRecipeFromUrlAsync(string url);

    /// <summary>
    /// Checks if a URL is supported for import
    /// </summary>
    bool IsSupportedUrl(string url);

    /// <summary>
    /// Gets list of supported recipe websites
    /// </summary>
    List<string> GetSupportedWebsites();
}
