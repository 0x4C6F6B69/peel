using Peel.Configuration;
using Peel.Domain;
using Peel.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PeachClient;
using Peel;

internal static class Factory
{
    public static PeachFacade CreateFacade() => new PeachFacade(CreateClient(), new Mapper());

    // public static OfferGrouper CreateGrouper() => new OfferGrouper(CreateProcessorConfig());

    public static PeachApiClient CreateClient() => new(NullLogger<PeachApiClient>.Instance,
        Options.Create(new PeachApiClientSettings {}));

    // private static IOptions<OfferProcessorConfig> CreateProcessorConfig() => Options.Create(new OfferProcessorConfig
    // {
    //     MaxDiffAmountBtcPc = 5,
    //     MaxDiffPremium = 1
    // });
}
