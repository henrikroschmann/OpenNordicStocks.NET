# OpenNordicStocks.NET

[![NuGet](https://img.shields.io/nuget/v/OpenNordicStocks.Client.svg)](https://www.nuget.org/packages/OpenNordicStocks.Client/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

OpenNordicStocks.NET is an open-source .NET service and SDK providing daily and historical stock data for Sweden, Denmark, and Finland (NASDAQ Nordic OMX). Free, cacheable, and CDN-ready — designed for developers and analysts who need reliable Nordic market data.

## 📦 Projects

This solution consists of four main projects:

### OpenNordicStocks.Core
Core library containing models and provider interfaces for Nordic stock data.

**Features:**
- `StockData` - Model representing individual stock information
- `StockSnapshot` - Model representing a complete market snapshot
- `IStockDataProvider` - Interface for implementing data providers

### OpenNordicStocks.Client
NuGet SDK for consuming stock data from the OpenNordicStocks CDN.

**Installation:**
```bash
dotnet add package OpenNordicStocks.Client
```

**Usage:**
```csharp
using OpenNordicStocks.Client;

var client = new OpenNordicStocksClient();

// Get latest stock data
var latestSnapshot = await client.GetLatestSnapshotAsync();
Console.WriteLine($"Total stocks: {latestSnapshot.Stocks.Count}");

// Get data for a specific date
var historicalSnapshot = await client.GetSnapshotForDateAsync(new DateTime(2025, 10, 28));
```

### OpenNordicStocks.Publisher
Console application that fetches stock data and publishes normalized JSON snapshots.

**Usage:**
```bash
export DATA_DIRECTORY=./data
dotnet run --project src/OpenNordicStocks.Publisher/OpenNordicStocks.Publisher.csproj
```

This project is designed to run as a GitHub Action on a daily schedule, automatically fetching and publishing stock data.

### OpenNordicStocks.Tests
xUnit test project containing unit tests for all components.

**Run tests:**
```bash
dotnet test
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later

### Building the Solution
```bash
dotnet restore
dotnet build
```

### Running Tests
```bash
dotnet test
```

## 📊 Data Format

Stock data is published as normalized JSON in the following format:

```json
{
  "date": "2025-10-28T00:00:00Z",
  "stocks": [
    {
      "symbol": "VOLV-B",
      "name": "Volvo AB",
      "price": 245.50,
      "market": "OMX Stockholm",
      "currency": "SEK",
      "timestamp": "2025-10-28T17:37:44.7434313Z",
      "volume": 1500000,
      "change": 2.50,
      "changePercent": 1.03
    }
  ],
  "metadata": {
    "version": "0.1.0",
    "generatedAt": "2025-10-28T17:37:44.743672Z",
    "totalCount": 3,
    "markets": ["OMX Stockholm", "OMX Helsinki", "OMX Copenhagen"]
  }
}
```

## 🌐 CDN Access

Published data will be available via CDN at:
- Latest snapshot: `https://cdn.opennordicstocks.net/data/latest.json`
- Historical data: `https://cdn.opennordicstocks.net/data/YYYY-MM-DD.json`

## 🤖 Automation

Stock data is automatically updated daily via GitHub Actions:
- Scheduled to run at 6:00 PM UTC (after Nordic markets close)
- Runs Monday through Friday (weekdays only)
- Can also be manually triggered via workflow dispatch

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Contact

Henrik Roschmann - [@henrikroschmann](https://github.com/henrikroschmann)

Project Link: [https://github.com/henrikroschmann/OpenNordicStocks.NET](https://github.com/henrikroschmann/OpenNordicStocks.NET)

