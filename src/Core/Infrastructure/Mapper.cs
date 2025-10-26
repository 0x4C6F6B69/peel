using System.Diagnostics;
using PeachClient.Models;
using Peel.Models;

namespace Peel.Infrastructure;

public class Mapper
{
    private const string SUMMARY_PREFIX = "smo-";
    public OfferSummaryType MapOfferType(OfferType type) => type switch
    {
        OfferType.Ask => OfferSummaryType.Sell,
        OfferType.Bid => OfferSummaryType.Buy,
        _ => throw new UnreachableException($"Unexpected value: {type}.")
    };

    public OfferSummary MapOffer(Offer offer, decimal btcUnitPrice)
    {
        var amountBtc = Converter.SatoshiToBitcoin((long)offer.Amount);
        var basePrice = amountBtc * btcUnitPrice;
        var price = offer.Type == OfferType.Ask
            ? basePrice.Increase(offer.Premium!.Value)
            : basePrice;
        OfferQuote quote = new()
        {
            AmountSat = (long)offer.Amount,
            AmountBtc = amountBtc,
            PriceFiat = Math.Round(price, 2),
            PriceFiatBase = offer.Type == OfferType.Ask ? Math.Round(basePrice, 2) : null
        };

        return new OfferSummary
        {
            Id = MapToSummaryId(offer.Id),
            ReferenceId = offer.Id,
            Type = MapOfferType(offer.Type),
            PublishingDate = offer.PublishingDate,
            Quote = quote,
            //QuoteMax = quoteMax,
            MeansOfPayment = offer.MeansOfPayment,
            SpreadPc = (float?)offer.Premium
        };
    }

    public string MapToSummaryId(long offerId) => $"{SUMMARY_PREFIX}{offerId}";

    public long MapToOfferId(string summaryId)
    {
        if (!summaryId.StartsWith(SUMMARY_PREFIX))
            throw new FormatException($"Summary ID must start with '{SUMMARY_PREFIX}'.");

        return Convert.ToInt64(summaryId.Substring(SUMMARY_PREFIX.Length));
    }
}
