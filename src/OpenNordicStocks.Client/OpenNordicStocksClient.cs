namespace OpenNordicStocks.Client;

using OpenNordicStocks.Core.Models;
using System.Text.Json;

/// <summary>
/// Client for consuming stock data from OpenNordicStocks CDN
/// </summary>
public class OpenNordicStocksClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the OpenNordicStocksClient
    /// </summary>
    /// <param name="httpClient">HTTP client for making requests</param>
    /// <param name="baseUrl">Base URL for the CDN (default: https://cdn.opennordicstocks.net)</param>
    public OpenNordicStocksClient(HttpClient? httpClient = null, string? baseUrl = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _baseUrl = baseUrl ?? "https://cdn.opennordicstocks.net";
    }

    /// <summary>
    /// Gets the latest stock data snapshot
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest stock snapshot</returns>
    public async Task<StockSnapshot?> GetLatestSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/data/latest.json";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<StockSnapshot>(content);
    }

    /// <summary>
    /// Gets stock data snapshot for a specific date
    /// </summary>
    /// <param name="date">The date to fetch data for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stock snapshot for the specified date</returns>
    public async Task<StockSnapshot?> GetSnapshotForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dateString = date.ToString("yyyy-MM-dd");
        var url = $"{_baseUrl}/data/{dateString}.json";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<StockSnapshot>(content);
    }
}
