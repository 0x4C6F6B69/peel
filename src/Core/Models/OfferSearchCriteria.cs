using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Peel.Models;

public enum OfferTypeFilter : byte
{
    All = 0,
    Sell,
    Buy,
}

public enum CurrencyType : byte
{
    Sat = 0,
    Btc,
    Fiat,
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

public abstract record class OfferSearchCriteria : IValidatableObject
{
    public required OfferTypeFilter OfferType { get; init; }
    public OfferAmount? Amount { get; set; }
    public float? MinSpread { get; set; }
    public float? MaxSpread { get; set; }
    public double? MinReputation { get; set; }
    public required CriteriaType Type { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        List<ValidationResult> validations = [];

        if (MinSpread != null && MaxSpread != null && MinSpread > MaxSpread) {
            validations.Add(new ValidationResult($"{nameof(MinSpread)} must be lesser than {nameof(MaxSpread)}",
                [nameof(MinSpread), nameof(MaxSpread)]));
        }

        // TODO: add a check on Amount when specifying a Buy offer (if Peach API does not change)

        return validations;
    }
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
