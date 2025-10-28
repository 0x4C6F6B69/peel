using System.ComponentModel.DataAnnotations;

namespace Peel.Configuration;

public sealed class BinanceConfig
{
    public const string SectionName = "binanceConfig";

    public Uri ApiEndpoint { get; set; } = new Uri("https://api.binance.com/api/v3");

    [Range(10, 300000, ErrorMessage = $"{nameof(TimeoutMs)} must range between 10 to 300000 milliseconds")]
    public int TimeoutMs { get; set; } = 5000;
}
