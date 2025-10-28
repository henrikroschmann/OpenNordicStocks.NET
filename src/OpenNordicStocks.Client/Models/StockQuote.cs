namespace OpenNordicStocks.Client.Models;

public class StockQuote
{
    public string FullName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;

    public decimal? NetChange { get; set; }
    public string PercentageChange { get; set; } = string.Empty;

    public decimal? BidPrice { get; set; }
    public decimal? AskPrice { get; set; }
    public decimal? LastSalePrice { get; set; }

    public decimal? High { get; set; }
    public decimal? Low { get; set; }

    public long? Volume { get; set; }
    public decimal? Turnover { get; set; }

    public string OrderbookId { get; set; } = string.Empty;
    public string AssetClass { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string DeltaIndicator { get; set; } = string.Empty;
}

