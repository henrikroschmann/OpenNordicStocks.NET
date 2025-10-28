using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using OpenNordicStocks.Client;
using OpenNordicStocks.Client.Models;
using RichardSzalay.MockHttp;

namespace OpenNordicStocks.Tests;

public class OpenNordicStocksClientTests
{
    private static HybridCache CreateTestCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<HybridCache>();
    }

    [Fact]
    public async Task GetRateAsync_WithNullDate_FetchesLatestData()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var testData = CreateTestStockQuotes();
        var json = JsonSerializer.Serialize(testData);

        mockHttp
            .When("https://cdn.jsdelivr.net/gh/henrikroschmann/OpenNordicStocks.NET@main/data/latest.json")
            .Respond("application/json", json);

        var httpClient = mockHttp.ToHttpClient();
        var cache = CreateTestCache();
        var client = new OpenNordicStocksClient(httpClient, cache);

        // Act
        var result = await client.GetRateAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Volvo AB", result[0].FullName);
    }

    [Fact]
    public async Task GetRateAsync_WithSpecificDate_FetchesHistoricalData()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var testData = CreateTestStockQuotes();
        var json = JsonSerializer.Serialize(testData);
        var testDate = new DateTime(2025, 10, 15);

        mockHttp
            .When("https://cdn.jsdelivr.net/gh/henrikroschmann/OpenNordicStocks.NET@main/data/2025-10-15.json")
            .Respond("application/json", json);

        var httpClient = mockHttp.ToHttpClient();
        var cache = CreateTestCache();
        var client = new OpenNordicStocksClient(httpClient, cache);

        // Act
        var result = await client.GetRateAsync(testDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRateAsync_HttpError_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("*")
            .Respond(HttpStatusCode.NotFound);

        var httpClient = mockHttp.ToHttpClient();
        var cache = CreateTestCache();
        var client = new OpenNordicStocksClient(httpClient, cache);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetRateAsync());
        
        Assert.Contains("Failed to fetch stock data", exception.Message);
    }

    [Fact]
    public async Task GetRateAsync_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("*")
            .Respond("application/json", "{ invalid json }");

        var httpClient = mockHttp.ToHttpClient();
        var cache = CreateTestCache();
        var client = new OpenNordicStocksClient(httpClient, cache);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetRateAsync());
        
        Assert.Contains("Failed to parse stock data", exception.Message);
    }

    [Fact]
    public async Task GetRateAsync_NullResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("*")
            .Respond("application/json", "null");

        var httpClient = mockHttp.ToHttpClient();
        var cache = CreateTestCache();
        var client = new OpenNordicStocksClient(httpClient, cache);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetRateAsync());
        
        Assert.Contains("Received null response", exception.Message);
    }

    [Fact]
    public async Task GetRateAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var testData = CreateTestStockQuotes();
        var json = JsonSerializer.Serialize(testData);

        mockHttp
            .When("*")
            .Respond(async (request) =>
            {
                await Task.Delay(1000);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                };
            });

        var httpClient = mockHttp.ToHttpClient();
        var cache = CreateTestCache();
        var client = new OpenNordicStocksClient(httpClient, cache);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetRateAsync(token: cts.Token));
    }

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        var cache = CreateTestCache();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OpenNordicStocksClient(null!, cache));
    }

    [Fact]
    public void Constructor_NullCache_ThrowsArgumentNullException()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OpenNordicStocksClient(httpClient, null!));
    }

    [Fact]
    public async Task GetRateAsync_CachesResults()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var testData = CreateTestStockQuotes();
        var json = JsonSerializer.Serialize(testData);
        var requestCount = 0;

        mockHttp
            .When("*")
            .Respond(_ =>
            {
                requestCount++;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

        var httpClient = mockHttp.ToHttpClient();
        var cache = CreateTestCache();
        var client = new OpenNordicStocksClient(httpClient, cache);

        // Act
        var result1 = await client.GetRateAsync();
        var result2 = await client.GetRateAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(1, requestCount); // Should only make one HTTP request due to caching
    }

    private static List<StockQuote> CreateTestStockQuotes()
    {
        return
        [
            new() {
                FullName = "Volvo AB",
                Symbol = "VOLV-B",
                Currency = "SEK",
                LastSalePrice = 245.50m,
                NetChange = 2.50m,
                PercentageChange = "1.03",
                OrderbookId = "SSE12345",
                AssetClass = "Stocks",
                Sector = "Industrials",
                Isin = "SE0000115420",
                DeltaIndicator = "Up",
                Volume = 1500000,
                High = 250.00m,
                Low = 240.00m
            },
            new() {
                FullName = "Nokia Corporation",
                Symbol = "NOKIA",
                Currency = "EUR",
                LastSalePrice = 3.45m,
                NetChange = -0.05m,
                PercentageChange = "-1.43",
                OrderbookId = "HEX67890",
                AssetClass = "Stocks",
                Sector = "Technology",
                Isin = "FI0009000681",
                DeltaIndicator = "Down",
                Volume = 5000000,
                High = 3.50m,
                Low = 3.40m
            }
        ];
    }
}
