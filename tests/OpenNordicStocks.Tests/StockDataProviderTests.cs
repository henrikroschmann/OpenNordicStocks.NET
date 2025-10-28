using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using OpenNordicStocks.Core.Models;
using OpenNordicStocks.Core.Providers;
using RichardSzalay.MockHttp;

namespace OpenNordicStocks.Tests;

public class StockDataProviderTests
{
    [Fact]
    public async Task FetchAsync_SinglePage_ReturnsAllStocks()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var testResponse = CreateScreenerResponse(50, 1, 1);
        var json = JsonSerializer.Serialize(testResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        mockHttp
            .When("https://api.nasdaq.com/api/nordic/screener/shares*")
            .Respond("application/json", json);

        var httpClient = mockHttp.ToHttpClient();
        var provider = new StockDataProvider(httpClient, NullLogger<StockDataProvider>.Instance);

        // Act
        var result = await provider.FetchAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Count);
    }

    [Fact]
    public async Task FetchAsync_MultiplePages_ReturnsAllStocks()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        // First page
        mockHttp
            .When(HttpMethod.Get, "https://api.nasdaq.com/api/nordic/screener/shares")
            .WithQueryString("page=1")
            .Respond("application/json", JsonSerializer.Serialize(
                CreateScreenerResponse(100, 1, 3),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        // Second page
        mockHttp
            .When(HttpMethod.Get, "https://api.nasdaq.com/api/nordic/screener/shares")
            .WithQueryString("page=2")
            .Respond("application/json", JsonSerializer.Serialize(
                CreateScreenerResponse(100, 2, 3),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        // Third page
        mockHttp
            .When(HttpMethod.Get, "https://api.nasdaq.com/api/nordic/screener/shares")
            .WithQueryString("page=3")
            .Respond("application/json", JsonSerializer.Serialize(
                CreateScreenerResponse(50, 3, 3),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var httpClient = mockHttp.ToHttpClient();
        var provider = new StockDataProvider(httpClient, NullLogger<StockDataProvider>.Instance);

        // Act
        var result = await provider.FetchAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(250, result.Count);
    }    [Fact]
    public async Task FetchAsync_EmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var testResponse = CreateScreenerResponse(0, 1, 1);
        var json = JsonSerializer.Serialize(testResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        mockHttp
            .When("*")
            .Respond("application/json", json);

        var httpClient = mockHttp.ToHttpClient();
        var provider = new StockDataProvider(httpClient, NullLogger<StockDataProvider>.Instance);

        // Act
        var result = await provider.FetchAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAsync_HttpError_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("*")
            .Respond(HttpStatusCode.InternalServerError);

        var httpClient = mockHttp.ToHttpClient();
        var provider = new StockDataProvider(httpClient, NullLogger<StockDataProvider>.Instance);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.FetchAsync());
        
        Assert.Contains("Failed to retrieve Nasdaq stocks", exception.Message);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task FetchAsync_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("*")
            .Respond("application/json", "{ invalid json }");

        var httpClient = mockHttp.ToHttpClient();
        var provider = new StockDataProvider(httpClient, NullLogger<StockDataProvider>.Instance);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.FetchAsync());
        
        Assert.Contains("Failed to parse Nasdaq stocks", exception.Message);
    }

    [Fact]
    public async Task FetchAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("*")
            .Respond(async (request) =>
            {
                await Task.Delay(5000);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var httpClient = mockHttp.ToHttpClient();
        var provider = new StockDataProvider(httpClient, NullLogger<StockDataProvider>.Instance);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.FetchAsync(cts.Token));
    }

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StockDataProvider(null!));
    }

    [Fact]
    public async Task FetchAsync_HandlesNullAndEmptyValues()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var response = new ScreenerResponse
        {
            Data = new ScreenerData
            {
                InstrumentListing = new InstrumentListing
                {
                    Rows =
                    [
                        new() {
                            FullName = "Test Company",
                            Symbol = "TEST",
                            Currency = "SEK",
                            LastSalePrice = null,
                            NetChange = null,
                            PercentageChange = "-",
                            Volume = null,
                            OrderbookId = "TEST123",
                            AssetClass = "Stocks",
                            Sector = "N/A",
                            Isin = "SE0000000000",
                            DeltaIndicator = ""
                        }
                    ],
                    TotalRecords = 1,
                    TotalPages = 1
                }
            }
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        mockHttp
            .When("*")
            .Respond("application/json", json);

        var httpClient = mockHttp.ToHttpClient();
        var provider = new StockDataProvider(httpClient, NullLogger<StockDataProvider>.Instance);

        // Act
        var result = await provider.FetchAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Null(result[0].LastSalePrice);
        Assert.Null(result[0].Volume);
    }

    [Fact]
    public async Task FetchAsync_StopsAtMaxPages()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        // Set up 150 pages of responses (more than MaxPages = 100)
        for (int i = 1; i <= 150; i++)
        {
            var pageNumber = i;
            mockHttp
                .When(HttpMethod.Get, "https://api.nasdaq.com/api/nordic/screener/shares")
                .WithQueryString($"page={pageNumber}")
                .Respond("application/json", JsonSerializer.Serialize(
                    CreateScreenerResponse(100, pageNumber, 150),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }

        var httpClient = mockHttp.ToHttpClient();
        var provider = new StockDataProvider(httpClient, NullLogger<StockDataProvider>.Instance);

        // Act
        var result = await provider.FetchAsync();

        // Assert
        Assert.NotNull(result);
        // Should stop at 100 pages, not fetch all 150
        Assert.Equal(10000, result.Count); // 100 pages * 100 items per page
    }    private static ScreenerResponse CreateScreenerResponse(int rowCount, int currentPage, int totalPages)
    {
        var rows = new List<StockQuote>();
        for (int i = 0; i < rowCount; i++)
        {
            rows.Add(new StockQuote
            {
                FullName = $"Company {i + 1}",
                Symbol = $"SYM{i + 1:D4}",
                Currency = "SEK",
                LastSalePrice = 100m + i,
                NetChange = 0.5m,
                PercentageChange = "0.5",
                Volume = 1000000 + i,
                OrderbookId = $"ORD{i + 1:D6}",
                AssetClass = "Stocks",
                Sector = "Technology",
                Isin = $"SE{i + 1:D10}",
                DeltaIndicator = "Up"
            });
        }

        return new ScreenerResponse
        {
            Data = new ScreenerData
            {
                InstrumentListing = new InstrumentListing
                {
                    Rows = rows,
                    TotalRecords = totalPages * 100,
                    TotalPages = totalPages
                }
            }
        };
    }
}
