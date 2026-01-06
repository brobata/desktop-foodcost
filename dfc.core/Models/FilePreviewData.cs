using System;
using System.Collections.Generic;

namespace Dfc.Core.Models;

/// <summary>
/// Contains preview data from an imported file for column mapping
/// </summary>
public class FilePreviewData
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Column headers from the file
    /// </summary>
    public List<string> Headers { get; set; } = new();

    /// <summary>
    /// Sample data rows (first N rows after header)
    /// </summary>
    public List<List<string>> SampleRows { get; set; } = new();

    /// <summary>
    /// Total number of data rows in the file (excluding header)
    /// </summary>
    public int TotalRowCount { get; set; }

    /// <summary>
    /// Which row was detected/selected as the header row (1-based)
    /// </summary>
    public int DetectedHeaderRow { get; set; } = 1;

    /// <summary>
    /// Detected delimiter for CSV files
    /// </summary>
    public string DetectedDelimiter { get; set; } = ",";

    /// <summary>
    /// Analyzed column information
    /// </summary>
    public List<FileColumn> Columns { get; set; } = new();

    /// <summary>
    /// Error message if file couldn't be loaded
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the file was successfully loaded
    /// </summary>
    public bool IsValid => string.IsNullOrEmpty(ErrorMessage) && Headers.Count > 0;
}

/// <summary>
/// Analyzed information about a single column in the import file
/// </summary>
public class FileColumn
{
    /// <summary>
    /// Zero-based index of the column
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Column header name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Sample values from this column (first N non-empty values)
    /// </summary>
    public List<string> SampleValues { get; set; } = new();

    /// <summary>
    /// Whether most values in this column appear to be numeric
    /// </summary>
    public bool LooksLikeNumber { get; set; }

    /// <summary>
    /// Whether this column appears to contain prices ($, decimal values)
    /// </summary>
    public bool LooksLikePrice { get; set; }

    /// <summary>
    /// Whether this column appears to contain SKUs (alphanumeric, consistent format)
    /// </summary>
    public bool LooksLikeSku { get; set; }

    /// <summary>
    /// Whether this column appears to contain combined quantity format (e.g., "6/5 LB")
    /// </summary>
    public bool LooksLikeCombinedQuantity { get; set; }

    /// <summary>
    /// Whether this column appears to contain unit values (LB, OZ, EA, etc.)
    /// </summary>
    public bool LooksLikeUnit { get; set; }

    /// <summary>
    /// Confidence score for auto-detection (0.0 - 1.0)
    /// </summary>
    public double DetectionConfidence { get; set; }

    /// <summary>
    /// Suggested DFC field mapping based on analysis
    /// </summary>
    public string? SuggestedMapping { get; set; }
}

/// <summary>
/// Represents a potential header row option for user selection
/// </summary>
public class HeaderRowOption
{
    /// <summary>
    /// 1-based row number
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Preview of the row content
    /// </summary>
    public string Preview { get; set; } = string.Empty;

    /// <summary>
    /// Whether this row was auto-detected as the likely header
    /// </summary>
    public bool IsDetected { get; set; }

    /// <summary>
    /// Individual cell values
    /// </summary>
    public List<string> Values { get; set; } = new();
}
