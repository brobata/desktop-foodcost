using Freecost.Core.Enums;

namespace Freecost.Core.Models;

public class PriceHistory : BaseEntity
{
    public Guid IngredientId { get; set; }
    public decimal Price { get; set; }
    public DateTime RecordedDate { get; set; }
    public bool IsAggregated { get; set; }
    public AggregationType AggregationType { get; set; }

    // v1.4.0 addition - Track which vendor this price came from
    public string? VendorName { get; set; }

    // Navigation properties
    public Ingredient Ingredient { get; set; } = null!;
}