using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PeachClient;
using Peel.Configuration;
using Peel.Infrastructure;
using Peel.Services;

internal static class Factory
{
    public static OfferReader CreateFacade() => new OfferReader(CreateClient(), new Mapper());

    public static PeachApiClient CreateClient() => new(NullLogger<PeachApiClient>.Instance,
        Options.Create(new PeachApiClientSettings { }));

    public static MarketAnalyzer CreateMarket() => new(CreateClient(),
        new BinanceClient(NullLogger<BinanceClient>.Instance,
        Options.Create(new BinanceConfig { })));
}
