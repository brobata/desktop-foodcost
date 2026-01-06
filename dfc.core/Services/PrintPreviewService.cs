using Dfc.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class PrintPreviewService : IPrintPreviewService
{
    private readonly IRecipeService _recipeService;
    private readonly IEntreeService _entreeService;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeCardService _recipeCardService;
    private readonly IEntreeCardService _entreeCardService;
    private readonly string _tempDirectory;

    public PrintPreviewService(
        IRecipeService recipeService,
        IEntreeService entreeService,
        IIngredientService ingredientService,
        IRecipeCardService recipeCardService,
        IEntreeCardService entreeCardService)
    {
        _recipeService = recipeService;
        _entreeService = entreeService;
        _ingredientService = ingredientService;
        _recipeCardService = recipeCardService;
        _entreeCardService = entreeCardService;

        _tempDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost",
            "PrintPreviews"
        );

        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
    }

    public async Task<byte[]> GenerateRecipePrintPreviewAsync(Guid recipeId, PrintSettings? settings = null)
    {
        settings ??= new PrintSettings();

        // Leverage existing RecipeCardService for recipe printing
        var recipe = await _recipeService.GetRecipeByIdAsync(recipeId);
        if (recipe == null)
        {
            throw new InvalidOperationException($"Recipe with ID {recipeId} not found");
        }

        var tempFilePath = Path.Combine(_tempDirectory, $"recipe_{recipeId}.pdf");
        await _recipeCardService.GenerateRecipeCardPdfAsync(recipe, tempFilePath);

        var pdfBytes = await File.ReadAllBytesAsync(tempFilePath);
        return pdfBytes;
    }

    public async Task<byte[]> GenerateEntreePrintPreviewAsync(Guid entreeId, PrintSettings? settings = null)
    {
        settings ??= new PrintSettings();

        // Leverage existing EntreeCardService for entree printing
        var entree = await _entreeService.GetEntreeByIdAsync(entreeId);
        if (entree == null)
        {
            throw new InvalidOperationException($"Entree with ID {entreeId} not found");
        }

        var tempFilePath = Path.Combine(_tempDirectory, $"entree_{entreeId}.pdf");
        await _entreeCardService.GenerateEntreeCardPdfAsync(entree, tempFilePath);

        var pdfBytes = await File.ReadAllBytesAsync(tempFilePath);
        return pdfBytes;
    }

    public async Task<byte[]> GenerateIngredientListPrintPreviewAsync(Guid locationId, string? category = null, PrintSettings? settings = null)
    {
        settings ??= new PrintSettings();

        var ingredients = await _ingredientService.GetAllIngredientsAsync(locationId);

        if (!string.IsNullOrEmpty(category))
        {
            ingredients = ingredients.Where(i => i.Category == category).ToList();
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Apply page size from settings
                page.Size(GetPageSize(settings.PageSize));

                // Apply orientation
                if (settings.Orientation == PageOrientation.Landscape)
                {
                    page.PageColor(Colors.White);
                }

                // Apply margins from settings
                page.Margin((float)settings.MarginTop, Unit.Inch);
                page.MarginBottom((float)settings.MarginBottom, Unit.Inch);
                page.MarginLeft((float)settings.MarginLeft, Unit.Inch);
                page.MarginRight((float)settings.MarginRight, Unit.Inch);

                page.DefaultTextStyle(x => x.FontSize(settings.BaseFontSize).FontFamily(settings.FontFamily));

                page.Header().Column(column =>
                {
                    if (settings.ShowHeader)
                    {
                        // Show company logo if enabled
                        if (settings.ShowCompanyLogo && !string.IsNullOrEmpty(settings.CompanyLogoPath) && File.Exists(settings.CompanyLogoPath))
                        {
                            column.Item().Height(50).AlignCenter().Image(settings.CompanyLogoPath);
                        }

                        // Show company name if provided
                        if (!string.IsNullOrEmpty(settings.CompanyName))
                        {
                            column.Item().Text(settings.CompanyName)
                                .FontSize(settings.SubHeaderFontSize)
                                .Bold()
                                .FontColor(settings.AccentColor);
                        }

                        column.Item().Text("Ingredient List")
                            .FontSize(settings.HeaderFontSize)
                            .Bold()
                            .FontColor(settings.UseColoredHeaders ? settings.HeaderColor : settings.TextColor);

                        if (!string.IsNullOrEmpty(category))
                        {
                            column.Item().Text($"Category: {category}")
                                .FontSize(settings.BaseFontSize + 2)
                                .Italic();
                        }

                        if (settings.ShowGeneratedDate)
                        {
                            column.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}")
                                .FontSize(settings.BaseFontSize - 1)
                                .FontColor(Colors.Grey.Darken1);
                        }

                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    }
                });

                page.Content().Column(column =>
                {
                    if (settings.GroupByCategory)
                    {
                        // Group by category
                        var groupedIngredients = ingredients.GroupBy(i => i.Category ?? "Uncategorized")
                            .OrderBy(g => g.Key);

                        foreach (var group in groupedIngredients)
                        {
                            column.Item().PaddingTop(15).Text(group.Key)
                                .FontSize(settings.SubHeaderFontSize)
                                .Bold()
                                .FontColor(settings.UseColoredHeaders ? settings.AccentColor : settings.TextColor);

                            column.Item().PaddingTop(5).Table(table =>
                            {
                                ConfigureIngredientTable(table, group.ToList(), settings);
                            });
                        }
                    }
                    else
                    {
                        // No grouping
                        column.Item().Table(table =>
                        {
                            ConfigureIngredientTable(table, ingredients, settings);
                        });
                    }

                    column.Item().PaddingTop(20).Text($"Total Ingredients: {ingredients.Count()}")
                        .FontSize(settings.BaseFontSize + 1)
                        .Bold();
                });

                if (settings.ShowFooter)
                {
                    page.Footer().AlignCenter().Text(text =>
                    {
                        if (settings.ShowPageNumbers)
                        {
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        }
                    });
                }
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateReportPrintPreviewAsync(string reportJson, PrintSettings? settings = null)
    {
        // Parse report data from JSON
        var reportData = JsonSerializer.Deserialize<Dictionary<string, object>>(reportJson);
        if (reportData == null)
        {
            throw new InvalidOperationException("Invalid report data");
        }

        var reportTitle = reportData.ContainsKey("title") ? reportData["title"].ToString() : "Custom Report";
        var reportDate = reportData.ContainsKey("date") ? reportData["date"].ToString() : DateTime.Now.ToString("MMMM dd, yyyy");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1, Unit.Inch);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Column(column =>
                {
                    column.Item().Text(reportTitle ?? "Report")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    column.Item().Text($"Generated: {reportDate}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(column =>
                {
                    column.Item().Text("Report data visualization will be implemented based on report type")
                        .FontSize(12);

                    // TODO: Add specific report rendering based on report type
                    // This is a basic implementation - can be extended for specific report types
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return await Task.FromResult(document.GeneratePdf());
    }

    public async Task<string> OpenPrintPreviewAsync(byte[] pdfData, string fileName, bool autoOpen = true)
    {
        var filePath = Path.Combine(_tempDirectory, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        await File.WriteAllBytesAsync(filePath, pdfData);

        // Open in default PDF viewer if requested
        if (autoOpen)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to open PDF: {ex.Message}", ex);
            }
        }

        return filePath;
    }

    // Helper methods

    private void ConfigureIngredientTable(TableDescriptor table, List<Ingredient> ingredients, PrintSettings settings)
    {
        // Column definition based on settings
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(3); // Name
            if (settings.ShowVendorInfo)
                columns.RelativeColumn(2); // Vendor
            columns.RelativeColumn(1); // Price
            if (settings.ShowUnitInfo)
                columns.RelativeColumn(1); // Unit
        });

        // Header
        table.Header(header =>
        {
            var headerBgColor = settings.UseColoredHeaders ? Colors.Grey.Lighten2 : Colors.White;

            header.Cell().Background(headerBgColor)
                .Padding(5).Text("Ingredient").Bold();

            if (settings.ShowVendorInfo)
            {
                header.Cell().Background(headerBgColor)
                    .Padding(5).Text("Vendor").Bold();
            }

            header.Cell().Background(headerBgColor)
                .Padding(5).Text("Price").Bold();

            if (settings.ShowUnitInfo)
            {
                header.Cell().Background(headerBgColor)
                    .Padding(5).Text("Unit").Bold();
            }
        });

        // Rows
        int rowIndex = 0;
        foreach (var ingredient in ingredients.OrderBy(i => i.Name))
        {
            var rowColor = settings.AlternateRowColors && rowIndex % 2 == 1
                ? Colors.Grey.Lighten3
                : Colors.White;

            var borderStyle = settings.UseTableBorders ? 0.5f : 0f;

            table.Cell().Background(rowColor)
                .BorderBottom(borderStyle).BorderColor(Colors.Grey.Lighten2)
                .Padding(5).Text(ingredient.Name);

            if (settings.ShowVendorInfo)
            {
                table.Cell().Background(rowColor)
                    .BorderBottom(borderStyle).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(ingredient.VendorName ?? "-");
            }

            table.Cell().Background(rowColor)
                .BorderBottom(borderStyle).BorderColor(Colors.Grey.Lighten2)
                .Padding(5).Text(ingredient.CurrentPrice > 0
                    ? $"${ingredient.CurrentPrice:F2}"
                    : "-");

            if (settings.ShowUnitInfo)
            {
                table.Cell().Background(rowColor)
                    .BorderBottom(borderStyle).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(ingredient.Unit.ToString());
            }

            rowIndex++;
        }
    }

    private dynamic GetPageSize(Models.PageSize pageSize)
    {
        return pageSize switch
        {
            Models.PageSize.Letter => PageSizes.Letter,
            Models.PageSize.Legal => PageSizes.Legal,
            Models.PageSize.A4 => PageSizes.A4,
            Models.PageSize.A5 => PageSizes.A5,
            _ => PageSizes.Letter
        };
    }
}
