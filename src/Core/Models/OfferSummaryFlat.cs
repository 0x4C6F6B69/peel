namespace Peel.Models;

public record class OfferSummaryFlat : OfferSummaryBase
{
    public required decimal QuoteAmountSat { get; init; }
    public required decimal QuoteAmountBtc { get; init; }
    public required decimal? QuotePriceFiatBase { get; init; }
    public required decimal QuotePriceFiat { get; init; }

    public decimal? QuoteMaxAmountSat { get; init; }
    public decimal? QuoteMaxAmountBtc { get; init; }
    public decimal? QuoteMaxPriceFiatBase { get; init; }
    public decimal? QuoteMaxPriceFiat { get; init; }

    public required string MeansOfPayment { get; init; }
}
