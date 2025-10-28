using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Hybrid;
using OpenNordicStocks.Client.Models;

namespace OpenNordicStocks.Client;

public sealed class OpenNordicStocksClient(HttpClient http, HybridCache cache) : IOpenNordicStocksClient
{
    private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly HybridCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    public async Task<List<StockQuote>> GetRateAsync(DateTime? at = null, CancellationToken token = default)
    {
        var effectiveDate = at?.Date ?? DateTime.UtcNow.Date;
        var key = $"opennordicstocks-{effectiveDate:yyyy-MM-dd}";

        return await _cache.GetOrCreateAsync(
            key,
            async cancel =>
            {
                var dateSegment = at?.ToString("yyyy-MM-dd") ?? "latest";
                var url = $"https://cdn.jsdelivr.net/gh/henrikroschmann/OpenNordicStocks.NET@main/data/{dateSegment}.json";

                try
                {
                    var stockQuotes = await _http.GetFromJsonAsync<List<StockQuote>>(url, cancel);

                    return stockQuotes is null
                        ? throw new InvalidOperationException(
                            $"Received null response when fetching stock data for {dateSegment}")
                        : stockQuotes;
                }
                catch (HttpRequestException ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to fetch stock data for {dateSegment}",
                        ex);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to parse stock data response for {dateSegment}",
                        ex);
                }
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(6),
                LocalCacheExpiration = TimeSpan.FromHours(1),
            },
            cancellationToken: token
        );
    }
}
