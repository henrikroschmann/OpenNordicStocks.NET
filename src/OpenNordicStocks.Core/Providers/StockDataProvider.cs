using Microsoft.Extensions.Logging;
using OpenNordicStocks.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNordicStocks.Core.Providers;

public class StockDataProvider(HttpClient http, ILogger<StockDataProvider>? logger = null)
{
    private const string NasdaqStocksUrl = "https://api.nasdaq.com/api/nordic/screener/shares";
    private const int DefaultPageSize = 100;
    private const int MaxPages = 100;
    
    private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly ILogger<StockDataProvider>? _logger = logger;
    private readonly JsonSerializerOptions _jsonOptions = CreateJsonOptions();

    public async Task<List<StockQuote>> FetchAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            List<StockQuote> allRows = [];
            int page = 1;
            int? totalPages = null;

            while (page <= MaxPages && (totalPages is null || page <= totalPages))
            {
                var url = $"{NasdaqStocksUrl}?category=MAIN_MARKET&tableonly=false&page={page}&size={DefaultPageSize}&lang=en";

                ScreenerResponse? response = await _http
                    .GetFromJsonAsync<ScreenerResponse>(url, _jsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                var listing = response?.Data?.InstrumentListing;
                var rows = listing?.Rows ?? [];

                if (rows.Count == 0)
                {
                    _logger?.LogDebug("No more rows returned at page {Page}", page);
                    break;
                }

                allRows.AddRange(rows);
                totalPages ??= listing?.TotalPages;

                _logger?.LogDebug(
                    "Fetched page {Page} of {TotalPages}, retrieved {RowCount} stocks",
                    page,
                    totalPages,
                    rows.Count);

                if (rows.Count < DefaultPageSize)
                {
                    break;
                }

                page++;
            }

            _logger?.LogInformation("Successfully fetched {TotalStocks} stocks", allRows.Count);
            return allRows;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Failed to fetch Nasdaq stocks from {Url}", NasdaqStocksUrl);
            throw new InvalidOperationException("Failed to retrieve Nasdaq stocks", ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Failed to parse Nasdaq stocks response");
            throw new InvalidOperationException("Failed to parse Nasdaq stocks", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not OperationCanceledException)
        {
            _logger?.LogError(ex, "Unexpected error while fetching Nasdaq stocks");
            throw new InvalidOperationException("Failed to fetch Nasdaq stocks", ex);
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        // Tolerant converters for mixed-format numeric fields
        jsonOptions.Converters.Add(new NullableDecimalConverter());
        jsonOptions.Converters.Add(new NullableLongConverter());
        return jsonOptions;
    }

    

    public sealed class NullableDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    return null;
                }

                s = s.Trim();
                if (s == "-" || s.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Remove spaces and normalize decimal separator to '.'
                s = s.Replace(" ", "");
                if (s.Contains(',') && s.Contains('.'))
                {
                    // Assume ',' is thousands separator when both present
                    s = s.Replace(",", "");
                }
                else if (s.Contains(','))
                {
                    // Treat ',' as decimal separator
                    s = s.Replace(',', '.');
                }

                // Some fields might carry percent sign even if mapped to decimal
                s = s.TrimEnd('%');

                if (decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var val))
                {
                    return val;
                }

                return null;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    public sealed class NullableLongConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    return null;
                }

                s = s.Trim();
                if (s == "-" || s.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Remove spaces and thousands separators, normalize sign
                s = s.Replace(" ", "").Replace(",", "");

                if (long.TryParse(s, NumberStyles.Integer | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var val))
                {
                    return val;
                }

                return null;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
