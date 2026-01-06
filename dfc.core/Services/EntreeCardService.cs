using Dfc.Core.Models;
using Dfc.Core.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IEntreeCardService
{
    Task<string> GenerateEntreeCardPdfAsync(Entree entree, string outputPath);
    Task<string> GenerateEntreeCardPdfAsync(Entree entree);
}

public class EntreeCardService : IEntreeCardService
{
    private readonly string _pdfOutputPath;

    public EntreeCardService()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _pdfOutputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Desktop Food Cost",
            "EntreeCards"
        );

        Directory.CreateDirectory(_pdfOutputPath);
    }

    public async Task<string> GenerateEntreeCardPdfAsync(Entree entree)
    {
        var fileName = $"{SanitizeFileName(entree.Name)}_{DateTime.Now:MMddyy}.pdf";
        var outputPath = Path.Combine(_pdfOutputPath, fileName);
        return await GenerateEntreeCardPdfAsync(entree, outputPath);
    }

    public async Task<string> GenerateEntreeCardPdfAsync(Entree entree, string outputPath)
    {
        return await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(c => ComposeHeader(c, entree));
                    page.Content().Element(c => ComposeContent(c, entree));
                    page.Footer().Element(c => ComposeFooter(c, entree));
                });
            })
            .GeneratePdf(outputPath);

            return outputPath;
        });
    }

    private void ComposeHeader(IContainer container, Entree entree)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(10).Text(entree.Name)
                .FontSize(24)
                .Bold()
                .FontColor(Colors.Grey.Darken4);

            if (!string.IsNullOrEmpty(entree.Description))
            {
                column.Item().PaddingBottom(10).Text(entree.Description)
                    .FontSize(12)
                    .Italic()
                    .FontColor(Colors.Grey.Darken2);
            }

            column.Item().PaddingBottom(5).Row(row =>
            {
                if (entree.MenuPrice.HasValue)
                {
                    row.RelativeItem().Text($"Menu Price: {entree.MenuPrice:C2}")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Colors.Green.Darken2);
                }

                row.RelativeItem().Text($"Food Cost: {entree.TotalCost:C2}")
                    .FontSize(11)
                    .SemiBold()
                    .FontColor(Colors.Orange.Darken2);

                row.RelativeItem().Text($"Cost %: {entree.FoodCostPercentage:F1}%")
                    .FontSize(11)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken2);
            });

            if (!string.IsNullOrEmpty(entree.Category))
            {
                column.Item().PaddingBottom(10).Text($"Category: {entree.Category}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            }

            column.Item().PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeContent(IContainer container, Entree entree)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Recipes Section
            if (entree.EntreeRecipes != null && entree.EntreeRecipes.Any())
            {
                column.Item().PaddingBottom(15).Element(c => ComposeRecipesSection(c, entree));
            }

            // Direct Ingredients Section
            if (entree.EntreeIngredients != null && entree.EntreeIngredients.Any())
            {
                column.Item().PaddingBottom(15).Element(c => ComposeIngredientsSection(c, entree));
            }

            // Plating Equipment
            if (!string.IsNullOrEmpty(entree.PlatingEquipment))
            {
                column.Item().PaddingBottom(10).Column(inner =>
                {
                    inner.Item().Text("Plating Equipment")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);

                    inner.Item().PaddingTop(5).Text(entree.PlatingEquipment)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken2);
                });
            }

            // Allergens Section
            if (entree.EntreeAllergens != null && entree.EntreeAllergens.Any(a => a.IsEnabled))
            {
                column.Item().PaddingTop(10).Element(c => ComposeAllergensSection(c, entree));
            }
        });
    }

    private void ComposeRecipesSection(IContainer container, Entree entree)
    {
        container.Column(column =>
        {
            column.Item().Text("Recipes")
                .FontSize(14)
                .Bold()
                .FontColor(Colors.Grey.Darken3);

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Recipe").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Quantity").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Unit").FontSize(10).SemiBold();
                });

                foreach (var er in entree.EntreeRecipes!)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(er.Recipe?.Name ?? "Unknown")
                        .FontSize(10);

                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{er.Quantity:F2}")
                        .FontSize(10);

                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(er.Unit.ToString())
                        .FontSize(10);
                }
            });
        });
    }

    private void ComposeIngredientsSection(IContainer container, Entree entree)
    {
        container.Column(column =>
        {
            column.Item().Text("Direct Ingredients")
                .FontSize(14)
                .Bold()
                .FontColor(Colors.Grey.Darken3);

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Ingredient").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Quantity").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Unit").FontSize(10).SemiBold();
                });

                foreach (var ei in entree.EntreeIngredients!)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(ei.Ingredient?.Name ?? "Unknown")
                        .FontSize(10);

                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{ei.Quantity:F2}")
                        .FontSize(10);

                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(ei.Unit.ToString())
                        .FontSize(10);
                }
            });
        });
    }

    private void ComposeAllergensSection(IContainer container, Entree entree)
    {
        container.Column(column =>
        {
            column.Item().Text("⚠️ Allergen Information")
                .FontSize(12)
                .Bold()
                .FontColor(Colors.Red.Darken2);

            var allergens = entree.EntreeAllergens!
                .Where(a => a.IsEnabled)
                .Select(a => a.Allergen.Name)
                .ToList();

            column.Item().PaddingTop(5).Text($"Contains: {string.Join(", ", allergens)}")
                .FontSize(10)
                .FontColor(Colors.Grey.Darken2);
        });
    }

    private void ComposeFooter(IContainer container, Entree entree)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Generated on ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.Span($"{DateTime.Now:MMMM dd, yyyy}").FontSize(8).SemiBold().FontColor(Colors.Grey.Darken1);
            text.Span(" • Desktop Food Cost").FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}
