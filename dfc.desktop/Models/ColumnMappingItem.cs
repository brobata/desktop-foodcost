using CommunityToolkit.Mvvm.ComponentModel;
using Dfc.Core.Models;
using System.Collections.Generic;

namespace Dfc.Desktop.Models;

/// <summary>
/// Represents a mapping between a DFC field and a file column
/// </summary>
public partial class ColumnMappingItem : ObservableObject
{
    /// <summary>
    /// Internal field identifier (e.g., "Name", "Price", "SKU")
    /// </summary>
    public string FieldId { get; set; } = string.Empty;

    /// <summary>
    /// Display label for the field (e.g., "Name *", "Price *")
    /// </summary>
    public string FieldLabel { get; set; } = string.Empty;

    /// <summary>
    /// Whether this field is required for import
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Currently selected column from the file
    /// </summary>
    [ObservableProperty]
    private string? _selectedColumn;

    /// <summary>
    /// Column that was auto-detected based on analysis
    /// </summary>
    [ObservableProperty]
    private string? _autoDetectedColumn;

    /// <summary>
    /// Whether the current mapping is valid
    /// </summary>
    [ObservableProperty]
    private bool _isValid;

    /// <summary>
    /// Validation message if not valid
    /// </summary>
    [ObservableProperty]
    private string? _validationMessage;

    /// <summary>
    /// Sample values from the selected column
    /// </summary>
    [ObservableProperty]
    private List<string> _sampleValues = new();

    /// <summary>
    /// Whether this field has special parsing options (quantity/size fields)
    /// </summary>
    public bool HasParsingOptions { get; set; }

    /// <summary>
    /// Whether the parsing options section is expanded
    /// </summary>
    [ObservableProperty]
    private bool _isParsingExpanded;

    /// <summary>
    /// Parse mode for quantity fields
    /// </summary>
    [ObservableProperty]
    private QuantityParseMode _parseMode = QuantityParseMode.Separate;

    /// <summary>
    /// Split character for combined format (e.g., "/")
    /// </summary>
    [ObservableProperty]
    private string _splitCharacter = "/";

    /// <summary>
    /// Detected format description (e.g., "Detected: 6/5 LB format")
    /// </summary>
    [ObservableProperty]
    private string? _detectedFormatDescription;

    /// <summary>
    /// Confidence level for auto-detection (0.0 - 1.0)
    /// </summary>
    public double DetectionConfidence { get; set; }

    /// <summary>
    /// Available columns from the file
    /// </summary>
    [ObservableProperty]
    private List<string> _availableColumns = new();

    /// <summary>
    /// Update validation state based on current selection
    /// </summary>
    public void Validate()
    {
        if (IsRequired && string.IsNullOrEmpty(SelectedColumn))
        {
            IsValid = false;
            ValidationMessage = $"{FieldLabel.TrimEnd('*', ' ')} is required";
        }
        else
        {
            IsValid = true;
            ValidationMessage = null;
        }
    }

    /// <summary>
    /// Create mapping items for all DFC fields
    /// </summary>
    public static List<ColumnMappingItem> CreateFieldMappings()
    {
        return new List<ColumnMappingItem>
        {
            // Required fields
            new ColumnMappingItem
            {
                FieldId = "Name",
                FieldLabel = "Name *",
                IsRequired = true
            },
            new ColumnMappingItem
            {
                FieldId = "Price",
                FieldLabel = "Price *",
                IsRequired = true
            },
            new ColumnMappingItem
            {
                FieldId = "SKU",
                FieldLabel = "SKU *",
                IsRequired = true
            },
            new ColumnMappingItem
            {
                FieldId = "Quantity",
                FieldLabel = "Quantity/Size",
                IsRequired = false,
                HasParsingOptions = true
            },
            new ColumnMappingItem
            {
                FieldId = "Unit",
                FieldLabel = "Unit",
                IsRequired = false
            },
            // Optional fields
            new ColumnMappingItem
            {
                FieldId = "Brand",
                FieldLabel = "Brand",
                IsRequired = false
            },
            new ColumnMappingItem
            {
                FieldId = "Vendor",
                FieldLabel = "Vendor",
                IsRequired = false
            },
            new ColumnMappingItem
            {
                FieldId = "Category",
                FieldLabel = "Category",
                IsRequired = false
            }
        };
    }
}
