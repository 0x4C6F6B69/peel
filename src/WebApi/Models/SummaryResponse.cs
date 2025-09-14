using Peel.Domain;

namespace Peel.Web.Models;

public class SummaryResponse<TSummary>
    where TSummary : OfferSummaryBase
{
    public required List<TSummary> Summaries { get; init; }

    public List<string>? Errors { get; init; }

    public required string DefaultFiat { get; init; }

    public required decimal BtcUnitPrice { get; init; }
}
