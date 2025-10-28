using OpenNordicStocks.Client.Models;

namespace OpenNordicStocks.Client;

public interface IOpenNordicStocksClient
{
    Task<List<StockQuote>> GetRateAsync(DateTime? at = null, CancellationToken token = default);
}
