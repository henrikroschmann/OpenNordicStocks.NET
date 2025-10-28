namespace OpenNordicStocks.Tests;

using OpenNordicStocks.Core.Models;
using OpenNordicStocks.Client;

public class StockDataTests
{
    [Fact]
    public void StockData_CanBeCreated()
    {
        var stockData = new StockData
        {
            Symbol = "TEST",
            Name = "Test Company",
            Price = 100.00m,
            Market = "OMX Stockholm",
            Currency = "SEK",
            Timestamp = DateTime.UtcNow
        };

        Assert.Equal("TEST", stockData.Symbol);
        Assert.Equal("Test Company", stockData.Name);
        Assert.Equal(100.00m, stockData.Price);
        Assert.Equal("OMX Stockholm", stockData.Market);
        Assert.Equal("SEK", stockData.Currency);
    }

    [Fact]
    public void StockSnapshot_CanBeCreated()
    {
        var snapshot = new StockSnapshot
        {
            Date = DateTime.UtcNow.Date,
            Stocks = new List<StockData>(),
            Metadata = new SnapshotMetadata
            {
                Version = "0.1.0",
                GeneratedAt = DateTime.UtcNow,
                TotalCount = 0,
                Markets = new List<string>()
            }
        };

        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.Stocks);
        Assert.NotNull(snapshot.Metadata);
        Assert.Equal("0.1.0", snapshot.Metadata.Version);
    }

    [Fact]
    public void OpenNordicStocksClient_CanBeInitialized()
    {
        var client = new OpenNordicStocksClient();
        Assert.NotNull(client);
    }
}

