// Location: Freecost.Core/Models/CostCalculationResult.cs
// Action: CREATE NEW FILE

namespace Freecost.Core.Models;

public class CostCalculationResult
{
    public decimal Cost { get; set; }
    public bool IsValid { get; set; }
    public string? WarningMessage { get; set; }
    public CostWarningLevel WarningLevel { get; set; }

    public static CostCalculationResult Success(decimal cost)
    {
        return new CostCalculationResult
        {
            Cost = cost,
            IsValid = true,
            WarningLevel = CostWarningLevel.None
        };
    }

    public static CostCalculationResult Warning(string message)
    {
        return new CostCalculationResult
        {
            Cost = 0m,
            IsValid = false,
            WarningMessage = message,
            WarningLevel = CostWarningLevel.MissingConversion
        };
    }

    public static CostCalculationResult Error(string message)
    {
        return new CostCalculationResult
        {
            Cost = 0m,
            IsValid = false,
            WarningMessage = message,
            WarningLevel = CostWarningLevel.Error
        };
    }
}

public enum CostWarningLevel
{
    None,
    MissingConversion,
    InvalidConversion,
    Error
}