using Dfc.Core.Models;
using System;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IPrintPreviewService
{
    /// <summary>
    /// Generate a print preview PDF for a recipe
    /// </summary>
    Task<byte[]> GenerateRecipePrintPreviewAsync(Guid recipeId, PrintSettings? settings = null);

    /// <summary>
    /// Generate a print preview PDF for an entree
    /// </summary>
    Task<byte[]> GenerateEntreePrintPreviewAsync(Guid entreeId, PrintSettings? settings = null);

    /// <summary>
    /// Generate a print preview PDF for an ingredient list
    /// </summary>
    Task<byte[]> GenerateIngredientListPrintPreviewAsync(Guid locationId, string? category = null, PrintSettings? settings = null);

    /// <summary>
    /// Generate a print preview PDF for a custom report
    /// </summary>
    Task<byte[]> GenerateReportPrintPreviewAsync(string reportJson, PrintSettings? settings = null);

    /// <summary>
    /// Open print preview in system default PDF viewer
    /// </summary>
    Task<string> OpenPrintPreviewAsync(byte[] pdfData, string fileName, bool autoOpen = true);
}
