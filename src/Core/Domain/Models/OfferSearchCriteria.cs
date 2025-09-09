using System.Text.Json.Serialization;

namespace Peel.Domain;

public enum OfferTypeFilter : byte
{
    All = 0,
    Sell,
    Buy
}

public enum CurrencyType : byte
{
    Btc = 0,
    Fiat
}

public record class OfferAmount(
    CurrencyType Currency,
    decimal AmountMin,
    decimal AmountMax
);

public enum CriteriaType : byte
{
    Default = 0,
    Advanced
}

// [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
// [JsonDerivedType(typeof(OfferSearchCriteriaDefault), "default")]
// [JsonDerivedType(typeof(OfferSearchCriteriaAdvanced), "advanced")]
public abstract record class OfferSearchCriteria
{
    public required OfferTypeFilter OfferType { get; init; }
    public OfferAmount? Amount { get; set; }
    public decimal? MaxPremium { get; set; }
    public double? MinReputation { get; set; }
    public required CriteriaType Type { get; init; }
}

public record class OfferSearchCriteriaDefault : OfferSearchCriteria
{
    public Dictionary<string, List<string>>? MeansOfPayment { get; set; }
}

public record class OfferSearchCriteriaAdvanced : OfferSearchCriteria
{
    public List<string>? PaymentFiat { get; set; }
    public List<string>? PaymentMethod { get; set; }
}
