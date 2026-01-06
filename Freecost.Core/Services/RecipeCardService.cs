using Freecost.Core.Models;
using Freecost.Core.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IRecipeCardService
{
    Task<string> GenerateRecipeCardPdfAsync(Recipe recipe, string outputPath);
    Task<string> GenerateRecipeCardPdfAsync(Recipe recipe); // Auto-generates filename
    Task<BatchPdfGenerationResult> GenerateBatchRecipeCardPdfsAsync(System.Collections.Generic.List<Recipe> recipes);
}

public class BatchPdfGenerationResult
{
    public int TotalRecipes { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public System.Collections.Generic.List<string> GeneratedFiles { get; set; } = new();
    public System.Collections.Generic.List<string> Errors { get; set; } = new();
}

public class RecipeCardService : IRecipeCardService
{
    private readonly string _pdfOutputPath;

    public RecipeCardService()
    {
        // Configure QuestPDF license (Community license for free use)
        QuestPDF.Settings.License = LicenseType.Community;

        // Set up PDF output directory
        _pdfOutputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Freecost",
            "RecipeCards"
        );

        Directory.CreateDirectory(_pdfOutputPath);
    }

    public async Task<string> GenerateRecipeCardPdfAsync(Recipe recipe)
    {
        var fileName = $"{SanitizeFileName(recipe.Name)}{DateTime.Now:MMddyy}.pdf";
        var outputPath = Path.Combine(_pdfOutputPath, fileName);
        return await GenerateRecipeCardPdfAsync(recipe, outputPath);
    }

    public async Task<string> GenerateRecipeCardPdfAsync(Recipe recipe, string outputPath)
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

                    page.Header().Element(c => ComposeHeader(c, recipe));
                    page.Content().Element(c => ComposeContent(c, recipe));
                    page.Footer().Element(c => ComposeFooter(c, recipe));
                });
            })
            .GeneratePdf(outputPath);

            return outputPath;
        });
    }

    private void ComposeHeader(IContainer container, Recipe recipe)
    {
        container.Column(column =>
        {
            // Recipe Name
            column.Item().PaddingBottom(10).Text(recipe.Name)
                .FontSize(24)
                .Bold()
                .FontColor(Colors.Grey.Darken4);

            // Description
            if (!string.IsNullOrEmpty(recipe.Description))
            {
                column.Item().PaddingBottom(10).Text(recipe.Description)
                    .FontSize(12)
                    .Italic()
                    .FontColor(Colors.Grey.Darken2);
            }

            // Yield and Prep Time
            column.Item().PaddingBottom(5).Row(row =>
            {
                row.RelativeItem().Text($"Yield: {recipe.Yield} {recipe.YieldUnit}")
                    .FontSize(11)
                    .SemiBold();

                if (recipe.PrepTimeMinutes.HasValue)
                {
                    row.RelativeItem().Text($"Prep Time: {recipe.PrepTimeMinutes} minutes")
                        .FontSize(11)
                        .SemiBold();
                }
            });

            // Allergen Warning Section
            var enabledAllergens = recipe.RecipeAllergens?
                .Where(a => a.IsEnabled && a.Allergen != null)
                .Select(a => a.Allergen.Type)
                .ToList();

            if (enabledAllergens?.Any() == true)
            {
                column.Item().PaddingTop(10).PaddingBottom(10)
                    .Background(Colors.Red.Lighten4)
                    .Padding(10)
                    .Column(allergenColumn =>
                    {
                        allergenColumn.Item().Text("⚠️ ALLERGEN WARNING")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Red.Darken2);

                        allergenColumn.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Contains: ");
                            text.Span(string.Join(", ", enabledAllergens.Select(FormatAllergenName)))
                                .SemiBold();
                        });
                    });
            }

            // Divider
            column.Item().PaddingTop(10).BorderBottom(2).BorderColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container, Recipe recipe)
    {
        container.PaddingTop(15).Column(column =>
        {
            // Ingredients Section
            column.Item().PaddingBottom(15).Column(ingredientSection =>
            {
                ingredientSection.Item().PaddingBottom(8).Text("INGREDIENTS")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Green.Darken2);

                if (recipe.RecipeIngredients?.Any() == true)
                {
                    var orderedIngredients = recipe.RecipeIngredients
                        .OrderBy(ri => ri.SortOrder)
                        .ToList();

                    foreach (var recipeIngredient in orderedIngredients)
                    {
                        ingredientSection.Item().PaddingBottom(4).Text(text =>
                        {
                            text.Span("• ");
                            text.Span($"{recipeIngredient.Quantity} {recipeIngredient.Unit} ");
                            text.Span(recipeIngredient.Ingredient?.Name ?? "Unknown");

                            if (recipeIngredient.IsOptional)
                            {
                                text.Span(" (optional)").Italic().FontColor(Colors.Grey.Medium);
                            }
                        });
                    }
                }
                else
                {
                    ingredientSection.Item().Text("No ingredients listed")
                        .Italic()
                        .FontColor(Colors.Grey.Medium);
                }
            });

            // Instructions Section
            if (!string.IsNullOrWhiteSpace(recipe.Instructions))
            {
                column.Item().PaddingTop(10).Column(instructionSection =>
                {
                    instructionSection.Item().PaddingBottom(8).Text("INSTRUCTIONS")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Green.Darken2);

                    instructionSection.Item().Text(recipe.Instructions)
                        .FontSize(11)
                        .LineHeight(1.5f);
                });
            }
        });
    }

    private void ComposeFooter(IContainer container, Recipe recipe)
    {
        container.AlignBottom().Column(column =>
        {
            column.Item().BorderTop(1).BorderColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Generated on {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);

                row.RelativeItem().AlignRight().Text("Powered by Freecost")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        });
    }

    private string FormatAllergenName(AllergenType allergen)
    {
        return allergen switch
        {
            AllergenType.TreeNuts => "Tree Nuts",
            AllergenType.GlutenFree => "Gluten-Free",
            AllergenType.ContainsAlcohol => "Contains Alcohol",
            _ => allergen.ToString()
        };
    }

    public async Task<BatchPdfGenerationResult> GenerateBatchRecipeCardPdfsAsync(System.Collections.Generic.List<Recipe> recipes)
    {
        var result = new BatchPdfGenerationResult
        {
            TotalRecipes = recipes.Count
        };

        foreach (var recipe in recipes)
        {
            try
            {
                var filePath = await GenerateRecipeCardPdfAsync(recipe);
                result.GeneratedFiles.Add(filePath);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{recipe.Name}: {ex.Message}");
                result.FailureCount++;
            }
        }

        return result;
    }

    private string SanitizeFileName(string fileName)
    {
        var invalids = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}
