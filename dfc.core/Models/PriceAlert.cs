using System;

namespace Dfc.Core.Models;

/// <summary>
/// Represents a price alert configuration for monitoring ingredient price changes
/// </summary>
public class PriceAlert : BaseEntity
{
    public Guid IngredientId { get; set; }
    public decimal ThresholdPercent { get; set; } // Alert if price changes by this %
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastAlertDate { get; set; }

    // Navigation
    public Ingredient Ingredient { get; set; } = null!;
}

/// <summary>
/// Represents an active price alert notification
/// </summary>
public class PriceAlertNotification
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal ChangePercent { get; set; }
    public DateTime ChangeDate { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum AlertSeverity
{
    Info,       // < 10% change
    Warning,    // 10-25% change
    Critical    // > 25% change
}
