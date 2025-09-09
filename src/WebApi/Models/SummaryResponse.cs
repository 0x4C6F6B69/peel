using Peel.Domain;

namespace Peel.Models;

public class SummaryResponse
{
    public required List<OfferSummary> Summaries { get; init; }

    public List<string>? Errors { get; init; }

    public required decimal BtcUnitPrice { get; init; }
}
