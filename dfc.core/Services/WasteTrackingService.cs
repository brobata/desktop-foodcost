using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class WasteTrackingService : IWasteTrackingService
{
    private readonly IWasteRecordRepository _wasteRecordRepository;
    private readonly IIngredientRepository _ingredientRepository;

    public WasteTrackingService(
        IWasteRecordRepository wasteRecordRepository,
        IIngredientRepository ingredientRepository)
    {
        _wasteRecordRepository = wasteRecordRepository;
        _ingredientRepository = ingredientRepository;
    }

    public async Task<WasteRecord> RecordWasteAsync(WasteRecord wasteRecord)
    {
        // Validate and calculate estimated cost if not provided
        if (wasteRecord.EstimatedCost == 0)
        {
            var ingredient = await _ingredientRepository.GetByIdAsync(wasteRecord.IngredientId);
            if (ingredient != null)
            {
                wasteRecord.EstimatedCost = wasteRecord.Quantity * ingredient.CurrentPrice;
            }
        }

        return await _wasteRecordRepository.AddAsync(wasteRecord);
    }

    public async Task<WasteReport> GenerateWasteReportAsync(Guid locationId, int daysBack = 30)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-daysBack);

        var wasteRecords = (await _wasteRecordRepository.GetByDateRangeAsync(locationId, startDate, endDate)).ToList();

        var report = new WasteReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalWasteRecords = wasteRecords.Count,
            TotalWasteCost = wasteRecords.Sum(wr => wr.EstimatedCost)
        };

        if (daysBack > 0)
        {
            report.AverageDailyCost = report.TotalWasteCost / daysBack;
        }

        // Group by ingredient
        var ingredientGroups = wasteRecords.GroupBy(wr => wr.IngredientId);
        var ingredientWasteSummaries = new List<IngredientWasteSummary>();

        foreach (var group in ingredientGroups)
        {
            var records = group.ToList();
            var ingredient = records.First().Ingredient;

            var mostCommonReason = records
                .GroupBy(wr => wr.Reason)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            var totalCost = records.Sum(wr => wr.EstimatedCost);
            var totalQuantity = records.Sum(wr => wr.Quantity);

            ingredientWasteSummaries.Add(new IngredientWasteSummary
            {
                IngredientId = group.Key,
                IngredientName = ingredient.Name,
                Category = ingredient.Category ?? "Uncategorized",
                TotalQuantityWasted = totalQuantity,
                Unit = records.First().Unit,
                TotalCost = totalCost,
                WasteCount = records.Count,
                MostCommonReason = mostCommonReason,
                Recommendation = GetWasteRecommendation(mostCommonReason, totalCost, records.Count)
            });
        }

        report.IngredientWaste = ingredientWasteSummaries
            .OrderByDescending(iws => iws.TotalCost)
            .ToList();

        // Group by waste reason
        var reasonGroups = wasteRecords.GroupBy(wr => wr.Reason);
        var wasteByReason = new Dictionary<WasteReason, WasteReasonSummary>();

        foreach (var reasonGroup in reasonGroups)
        {
            var reasonRecords = reasonGroup.ToList();
            var totalCost = reasonRecords.Sum(wr => wr.EstimatedCost);
            var percentage = report.TotalWasteCost > 0
                ? (totalCost / report.TotalWasteCost) * 100
                : 0;

            wasteByReason[reasonGroup.Key] = new WasteReasonSummary
            {
                Reason = reasonGroup.Key,
                Count = reasonRecords.Count,
                TotalCost = totalCost,
                Percentage = percentage,
                Recommendation = GetReasonRecommendation(reasonGroup.Key, percentage)
            };
        }

        report.WasteByReason = wasteByReason;

        // Determine trend (compare to previous period)
        var previousStartDate = startDate.AddDays(-daysBack);
        var previousWasteRecords = (await _wasteRecordRepository.GetByDateRangeAsync(locationId, previousStartDate, startDate)).ToList();
        var previousTotalCost = previousWasteRecords.Sum(wr => wr.EstimatedCost);

        if (previousTotalCost == 0)
        {
            report.Trend = WasteTrend.Stable;
        }
        else
        {
            var changePercent = ((report.TotalWasteCost - previousTotalCost) / previousTotalCost) * 100;

            if (changePercent < -10)
                report.Trend = WasteTrend.Improving;
            else if (changePercent > 10)
                report.Trend = WasteTrend.Worsening;
            else
                report.Trend = WasteTrend.Stable;
        }

        return report;
    }

    public async Task<List<IngredientWasteSummary>> GetTopWastedIngredientsAsync(Guid locationId, int count = 20)
    {
        var report = await GenerateWasteReportAsync(locationId);
        return report.IngredientWaste.Take(count).ToList();
    }

    public async Task<Dictionary<WasteReason, decimal>> GetWasteByReasonAsync(Guid locationId, int daysBack = 30)
    {
        var report = await GenerateWasteReportAsync(locationId, daysBack);
        return report.WasteByReason.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.TotalCost
        );
    }

    private string GetWasteRecommendation(WasteReason reason, decimal totalCost, int wasteCount)
    {
        if (totalCost < 10)
            return "✓ Minimal waste. Continue current practices.";

        return reason switch
        {
            WasteReason.Spoilage =>
                $"⚠️ ${totalCost:F2} lost to spoilage ({wasteCount} incidents). Review storage practices and rotation (FIFO). Consider reducing par levels.",

            WasteReason.Expired =>
                $"⚠️ ${totalCost:F2} lost to expiration ({wasteCount} incidents). Improve inventory rotation. Order smaller quantities more frequently.",

            WasteReason.Overproduction =>
                $"⚠️ ${totalCost:F2} lost to overproduction ({wasteCount} incidents). Better forecast demand. Adjust prep quantities. Implement JIT production.",

            WasteReason.Preparation =>
                $"⚠️ ${totalCost:F2} lost in preparation ({wasteCount} incidents). Review prep techniques. Provide additional training. Use trim more efficiently.",

            WasteReason.Contamination =>
                $"❌ ${totalCost:F2} lost to contamination ({wasteCount} incidents). Critical: Review food safety procedures immediately. Retrain staff.",

            WasteReason.Damage =>
                $"⚠️ ${totalCost:F2} lost to damage ({wasteCount} incidents). Improve handling procedures. Review storage conditions. Check packaging quality.",

            _ =>
                $"⚠️ ${totalCost:F2} wasted ({wasteCount} incidents). Investigate root causes and implement corrective actions."
        };
    }

    private string GetReasonRecommendation(WasteReason reason, decimal percentage)
    {
        var severity = percentage switch
        {
            > 30 => "Critical",
            > 15 => "High priority",
            > 5 => "Moderate priority",
            _ => "Low priority"
        };

        return reason switch
        {
            WasteReason.Spoilage =>
                $"{severity}: {percentage:F1}% of waste. Implement better cold storage practices and FIFO rotation.",

            WasteReason.Expired =>
                $"{severity}: {percentage:F1}% of waste. Improve inventory management. Date-label all items. Reduce order sizes.",

            WasteReason.Overproduction =>
                $"{severity}: {percentage:F1}% of waste. Match production to actual demand. Use production logs and sales data.",

            WasteReason.Preparation =>
                $"{severity}: {percentage:F1}% of waste. Train staff on efficient prep techniques. Use equipment properly.",

            WasteReason.Contamination =>
                $"CRITICAL: {percentage:F1}% of waste. Review all food safety protocols immediately. Mandatory retraining.",

            WasteReason.Damage =>
                $"{severity}: {percentage:F1}% of waste. Improve handling and storage systems.",

            _ =>
                $"{severity}: {percentage:F1}% of waste. Requires investigation."
        };
    }
}
