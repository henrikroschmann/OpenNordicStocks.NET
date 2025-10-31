using OpenNordicStocks.Client;

var services = new ServiceCollection();

services.AddHttpClient<IOpenNordicStocksClient, OpenNordicStocksClient>();
services.AddHybridCache();

var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IOpenNordicStocksClient>();

// Get latest stock data
var latestSnapshot = await client.GetRateAsync();

Console.WriteLine(latestSnapshot);
