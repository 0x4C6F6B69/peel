namespace Peel.Domain;

public record class OfferQuote
{
    public long AmountSat { get; init; }
    public decimal AmountBtc { get; init; }
    /// <summary>
    /// Represents the base price excluding the Premium spread increment (applicable only to Sell offers).
    /// </summary>
    public decimal? PriceFiatBase { get; init; }
    public decimal PriceFiat { get; init; }
}

public enum OfferSummaryType
{
    Sell, // ask
    Buy // bid
}

/// <summary>
/// Contains properties in common form default and flat derivates.
/// </summary>
public abstract record class OfferSummaryBase
{
    public required string Id { get; init; }
    public required string ReferenceId { get; init; }
    public required OfferSummaryType Type { get; init; }
    public DateTime? PublishingDate { get; set; }
    public float? SpreadPc { get; set; }
}

public record class OfferSummary : OfferSummaryBase
{
    public required OfferQuote Quote { get; init; }
    public OfferQuote? QuoteMax { get; set; }
    public required Dictionary<string, List<string>> MeansOfPayment { get; init; }
}
