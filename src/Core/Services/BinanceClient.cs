using Peel.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Peel.Services;

public sealed class BinanceClient
{
    private readonly ILogger _logger;
    private readonly BinanceConfig _config;
    private readonly RestClient _client;

    public BinanceClient(ILogger<BinanceClient> logger, IOptions<BinanceConfig> options)
    {
        _logger = logger;
        _config = options.Value;

        _client = new RestClient(new RestClientOptions
        {
            BaseUrl = _config.ApiEndpoint,
            Timeout = TimeSpan.FromMilliseconds(_config.TimeoutMs)
        });
    }

    public async Task<List<List<object>>> GetCandlesAsync(string pair, string interval, int limit)
    {
        var request = new RestRequest("klines", Method.Get)
            .AddParameter("symbol", pair)
            .AddParameter("interval", interval)
            .AddParameter("limit", limit);

        try {
            var response = await _client.ExecuteAsync<List<List<object>>>(request);
            if (response?.Data != null)
                return response.Data;
        }
        catch (Exception ex) {
            _logger.LogCritical(ex, "Failed to retrieve Binance klines");
        }

        return [];
    }
}
