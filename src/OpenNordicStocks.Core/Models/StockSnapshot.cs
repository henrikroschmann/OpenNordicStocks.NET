namespace OpenNordicStocks.Core.Models;

/// <summary>
/// Represents a snapshot of stock data for a specific date
/// </summary>
public class StockSnapshot
{
    /// <summary>
    /// Date of the snapshot
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Collection of stock data
    /// </summary>
    public required List<StockData> Stocks { get; set; }

    /// <summary>
    /// Snapshot metadata
    /// </summary>
    public required SnapshotMetadata Metadata { get; set; }
}

/// <summary>
/// Metadata for a stock data snapshot
/// </summary>
public class SnapshotMetadata
{
    /// <summary>
    /// Version of the data schema
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Time when the snapshot was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Total number of stocks in the snapshot
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Markets included in the snapshot
    /// </summary>
    public required List<string> Markets { get; set; }
}
