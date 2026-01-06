using System;

namespace Freecost.Core.Models;

public class UserPreferences : BaseEntity
{
    public Guid UserId { get; set; }

    // Display preferences
    public string Theme { get; set; } = "Light"; // Light, Dark
    public string Language { get; set; } = "en-US";
    public int PageSize { get; set; } = 50;
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string TimeFormat { get; set; } = "12h"; // 12h, 24h
    public string CurrencySymbol { get; set; } = "$";

    // Notification preferences
    public bool EmailNotifications { get; set; } = true;
    public bool PriceAlertNotifications { get; set; } = true;
    public bool ApprovalNotifications { get; set; } = true;
    public bool CommentNotifications { get; set; } = true;

    // Dashboard preferences
    public string DefaultDashboard { get; set; } = "Executive Summary";
    public string DefaultView { get; set; } = "Dashboard"; // Dashboard, Ingredients, Recipes, etc.

    // Recipe preferences
    public string DefaultRecipeSort { get; set; } = "Name";
    public string DefaultRecipeFilter { get; set; } = "All";
    public bool ShowRecipeCosts { get; set; } = true;
    public bool ShowNutritionalInfo { get; set; } = true;

    // Export preferences
    public string DefaultExportFormat { get; set; } = "Excel"; // Excel, CSV, PDF

    // Auto-save preferences
    public bool AutoSaveEnabled { get; set; } = true;
    public int AutoSaveIntervalSeconds { get; set; } = 30;

    // Onboarding/User info (for community/email list)
    public string? UserName { get; set; }
    public string? RestaurantName { get; set; }
    public string? Email { get; set; }
    public bool AgreeToNewsletter { get; set; } = true; // Always true - required info

    // Print preferences (stored as JSON)
    public string? PrintSettingsJson { get; set; }

    // Helper property to deserialize print settings
    public PrintSettings GetPrintSettings()
    {
        if (string.IsNullOrEmpty(PrintSettingsJson))
            return new PrintSettings();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<PrintSettings>(PrintSettingsJson) ?? new PrintSettings();
        }
        catch
        {
            return new PrintSettings();
        }
    }

    public void SetPrintSettings(PrintSettings settings)
    {
        PrintSettingsJson = System.Text.Json.JsonSerializer.Serialize(settings);
    }

    // Navigation
    public User? User { get; set; }
}
