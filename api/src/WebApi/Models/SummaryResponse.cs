using Peel.Models;

namespace Peel.Web.Models;

public abstract class SummaryResponseBase
{
    public List<string>? Errors { get; init; }

    public required string DefaultFiat { get; init; }

    public required decimal BtcUnitPrice { get; init; }
}

public class SummaryResponse<TSummary> : SummaryResponseBase
    where TSummary : OfferSummaryBase
{
    public required List<TSummary> Summaries { get; init; }
}

public class SummaryGroupResponse<TGroup> : SummaryResponseBase
    where TGroup : OfferSummaryGroup
{
    public required List<TGroup> Groups { get; init; }
}
