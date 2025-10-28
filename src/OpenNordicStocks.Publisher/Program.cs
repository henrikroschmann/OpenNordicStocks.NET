using OpenNordicStocks.Core.Models;
using System.Text.Json;

Console.WriteLine("OpenNordicStocks.Publisher - Nordic Stock Data Fetcher");
Console.WriteLine("======================================================");
Console.WriteLine();

// This is a placeholder implementation for the GitHub Action data fetcher
// In a production environment, this would:
// 1. Fetch stock data from Nordic exchanges (OMX Stockholm, Helsinki, Copenhagen)
// 2. Normalize the data into the StockSnapshot format
// 3. Save it as JSON to the /data directory
// 4. Be scheduled to run daily via GitHub Actions

try
{
    // Get data directory from environment variable or use default
    var dataDirectory = Environment.GetEnvironmentVariable("DATA_DIRECTORY");
    
    if (string.IsNullOrEmpty(dataDirectory))
    {
        // When running from dotnet run, use the solution root
        var currentDir = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", ".."));
        dataDirectory = Path.Combine(solutionRoot, "data");
    }
    
    if (!Directory.Exists(dataDirectory))
    {
        Directory.CreateDirectory(dataDirectory);
    }
    
    Console.WriteLine($"Data directory: {dataDirectory}");
    
    // Create a sample snapshot
    var snapshot = CreateSampleSnapshot();
    
    // Save to file
    var fileName = $"{DateTime.UtcNow:yyyy-MM-dd}.json";
    var filePath = Path.Combine(dataDirectory, fileName);
    
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    var json = JsonSerializer.Serialize(snapshot, options);
    await File.WriteAllTextAsync(filePath, json);
    
    // Also save as latest.json
    var latestPath = Path.Combine(dataDirectory, "latest.json");
    await File.WriteAllTextAsync(latestPath, json);
    
    Console.WriteLine($"Successfully saved snapshot to {fileName}");
    Console.WriteLine($"Total stocks: {snapshot.Stocks.Count}");
    Console.WriteLine($"Markets: {string.Join(", ", snapshot.Metadata.Markets)}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

static StockSnapshot CreateSampleSnapshot()
{
    // This is sample data - in production, this would fetch real data from exchanges
    return new StockSnapshot
    {
        Date = DateTime.UtcNow.Date,
        Stocks = new List<StockData>
        {
            new StockData
            {
                Symbol = "VOLV-B",
                Name = "Volvo AB",
                Price = 245.50m,
                Market = "OMX Stockholm",
                Currency = "SEK",
                Timestamp = DateTime.UtcNow,
                Volume = 1500000,
                Change = 2.50m,
                ChangePercent = 1.03m
            },
            new StockData
            {
                Symbol = "NOKIA",
                Name = "Nokia Corporation",
                Price = 3.45m,
                Market = "OMX Helsinki",
                Currency = "EUR",
                Timestamp = DateTime.UtcNow,
                Volume = 5000000,
                Change = -0.05m,
                ChangePercent = -1.43m
            },
            new StockData
            {
                Symbol = "NOVO-B",
                Name = "Novo Nordisk B",
                Price = 825.00m,
                Market = "OMX Copenhagen",
                Currency = "DKK",
                Timestamp = DateTime.UtcNow,
                Volume = 800000,
                Change = 12.50m,
                ChangePercent = 1.54m
            }
        },
        Metadata = new SnapshotMetadata
        {
            Version = "0.1.0",
            GeneratedAt = DateTime.UtcNow,
            TotalCount = 3,
            Markets = new List<string> { "OMX Stockholm", "OMX Helsinki", "OMX Copenhagen" }
        }
    };
}

