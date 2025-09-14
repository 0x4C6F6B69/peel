namespace Peel.Models;

public abstract record class OfferSummaryGroup(
    List<OfferSummary> Summaries
);

public record class OfferSummaryBySpreadGroup(
    float? SpreadPc,
    List<OfferSummary> Summaries
) : OfferSummaryGroup(Summaries);

public record class OfferSummaryByPriceFiatGroup(
    decimal PriceFiat,
    List<OfferSummary> Summaries
) : OfferSummaryGroup(Summaries);

