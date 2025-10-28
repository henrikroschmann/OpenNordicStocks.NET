namespace OpenNordicStocks.Core.Models;

/// <summary>
/// Represents stock data for a single security
/// </summary>
public class StockData
{
    /// <summary>
    /// Stock ticker symbol
    /// </summary>
    public required string Symbol { get; set; }

    /// <summary>
    /// Company name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Current or closing price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Market (e.g., "OMX Stockholm", "OMX Helsinki", "OMX Copenhagen")
    /// </summary>
    public required string Market { get; set; }

    /// <summary>
    /// Currency code (e.g., SEK, EUR, DKK)
    /// </summary>
    public required string Currency { get; set; }

    /// <summary>
    /// Timestamp of the data
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Trading volume
    /// </summary>
    public long? Volume { get; set; }

    /// <summary>
    /// Change in price
    /// </summary>
    public decimal? Change { get; set; }

    /// <summary>
    /// Percentage change
    /// </summary>
    public decimal? ChangePercent { get; set; }
}
