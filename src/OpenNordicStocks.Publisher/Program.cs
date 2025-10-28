using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenNordicStocks.Core.Providers;
using System.Net.Http.Headers;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<StockDataProvider>(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);

    options.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36");
    options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    options.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
    options.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.All
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting OpenNordic Stocks Publisher...");

using var cts = new CancellationTokenSource();

// Register cancellation handler for Ctrl+C
Console.CancelKeyPress += (sender, e) =>
{
    logger.LogWarning("Cancellation requested by user");
    e.Cancel = true;
    cts.Cancel();
};

try
{
    var stockDataProvider = host.Services.GetRequiredService<StockDataProvider>();
    var stocks = await stockDataProvider.FetchAsync(cts.Token);

    // Ensure data folder exists
    Directory.CreateDirectory("data");

    var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
    var latestPath = Path.Combine("data", "latest.json");
    var datedPath = Path.Combine("data", $"{date}.json");

    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    var json = JsonSerializer.Serialize(stocks, jsonOptions);

    await File.WriteAllTextAsync(latestPath, json, cts.Token);
    logger.LogInformation("Stock data written to: {Path}", latestPath);

    await File.WriteAllTextAsync(datedPath, json, cts.Token);
    logger.LogInformation("Stock data written to: {Path}", datedPath);

    logger.LogInformation("Successfully updated {StockCount} stocks for {Date}", stocks.Count, date);

    return 0;
}
catch (OperationCanceledException)
{
    logger.LogWarning("Operation cancelled by user");
    return 1;
}
catch (IOException ex)
{
    logger.LogError(ex, "I/O error occurred while writing stock data files");
    return 1;
}
catch (Exception ex)
{
    logger.LogError(ex, "Fatal error occurred while publishing stock data");
    return 1;
}