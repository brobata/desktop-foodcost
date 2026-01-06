using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Dfc.Core.Models;
using Dfc.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.Views;

public partial class ImportRecipeFromUrlWindow : Window
{
    private readonly IRecipeUrlImportService? _importService;
    public Recipe? ImportedRecipe { get; private set; }
    public bool WasImported { get; private set; }

    // Parameterless constructor for XAML designer
    public ImportRecipeFromUrlWindow() : this(null)
    {
    }

    public ImportRecipeFromUrlWindow(IRecipeUrlImportService? importService)
    {
        InitializeComponent();
        _importService = importService;

        // Populate supported websites list if service is provided
        if (_importService != null)
        {
            var websites = _importService.GetSupportedWebsites();
            SupportedWebsitesList.ItemsSource = websites;
        }
    }

    private async void OnImport(object? sender, RoutedEventArgs e)
    {
        var url = UrlTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(url))
        {
            ShowStatus("⚠️", "Please enter a recipe URL", "#FFF3E0", "#FF9800");
            return;
        }

        // Validate URL format
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            ShowStatus("❌", "Please enter a valid URL (starting with http:// or https://)", "#FFEBEE", "#EF5350");
            return;
        }

        // Check if service is available
        if (_importService == null)
        {
            ShowStatus("❌", "Import service is not available", "#FFEBEE", "#EF5350");
            return;
        }

        // Check if URL is supported
        if (!_importService.IsSupportedUrl(url))
        {
            ShowStatus("⚠️", $"This website may not be supported. Import may fail or produce incomplete results.", "#FFF3E0", "#FF9800");
            await Task.Delay(2000); // Show warning briefly before proceeding
        }

        // Show loading state
        LoadingBorder.IsVisible = true;
        StatusBorder.IsVisible = false;
        ImportButton.IsEnabled = false;
        UrlTextBox.IsEnabled = false;

        try
        {
            // Attempt to import
            var recipe = await _importService.ImportRecipeFromUrlAsync(url);

            if (recipe != null && !string.IsNullOrWhiteSpace(recipe.Name))
            {
                ImportedRecipe = recipe;
                WasImported = true;

                ShowStatus("✅", $"Successfully imported '{recipe.Name}'!", "#E8F5E9", "#4CAF50");
                await Task.Delay(1000);

                Close();
            }
            else
            {
                ShowStatus("❌", "Could not import recipe. The website may not be supported or the URL may not point to a recipe.", "#FFEBEE", "#EF5350");
                ImportButton.IsEnabled = true;
                UrlTextBox.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            ShowStatus("❌", $"Import failed: {ex.Message}", "#FFEBEE", "#EF5350");
            ImportButton.IsEnabled = true;
            UrlTextBox.IsEnabled = true;
        }
        finally
        {
            LoadingBorder.IsVisible = false;
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        WasImported = false;
        Close();
    }

    private void ShowStatus(string icon, string message, string backgroundColor, string borderColor)
    {
        StatusIcon.Text = icon;
        StatusText.Text = message;
        StatusBorder.Background = Brush.Parse(backgroundColor);
        StatusBorder.BorderBrush = Brush.Parse(borderColor);
        StatusBorder.IsVisible = true;
    }
}
