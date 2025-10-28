namespace OpenNordicStocks.Core.Providers;

using OpenNordicStocks.Core.Models;

/// <summary>
/// Interface for stock data providers
/// </summary>
public interface IStockDataProvider
{
    /// <summary>
    /// Fetches current stock data for all supported markets
    /// </summary>
    /// <returns>A stock snapshot containing current data</returns>
    Task<StockSnapshot> FetchCurrentDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches stock data for a specific date
    /// </summary>
    /// <param name="date">The date for which to fetch data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A stock snapshot for the specified date</returns>
    Task<StockSnapshot> FetchDataForDateAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of supported markets
    /// </summary>
    /// <returns>List of market names</returns>
    List<string> GetSupportedMarkets();
}
