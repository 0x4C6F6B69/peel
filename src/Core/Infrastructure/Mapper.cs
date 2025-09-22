using Peel;
using PeachClient.Models;
using System.Diagnostics;
using SharpX;
using Peel.Models;

namespace Peel.Infrastructure;

public class Mapper
{
    private const string SUMMARY_PREFIX = "smo-";

    public PeachClient.Models.OfferTypeFilter MapOfferTypeFilter(Peel.Models.OfferTypeFilter filterType) => filterType switch
    {
        Peel.Models.OfferTypeFilter.All => PeachClient.Models.OfferTypeFilter.All,
        Peel.Models.OfferTypeFilter.Sell => PeachClient.Models.OfferTypeFilter.Ask,
        Peel.Models.OfferTypeFilter.Buy => PeachClient.Models.OfferTypeFilter.Bid,
        _ => throw new UnreachableException($"Unexpected value: {filterType}.")
    };

    public OfferFilter MapOfferFilter(OfferSearchCriteria criteria, decimal btcUnitPrice)
    {
        OfferFilter filter = new()
        {
            Type = MapOfferTypeFilter(criteria.OfferType),
            MaxPremium = (decimal?)criteria.MaxSpread,
            MinReputation = criteria.MinReputation
        };

        if (criteria.Amount != null) {
            var satsMin = criteria.Amount.Currency switch
            {
                CurrencyType.Sat  => (long)criteria.Amount.AmountMin,
                CurrencyType.Btc  => Converter.BitcoinToSatoshi(criteria.Amount.AmountMin / btcUnitPrice),
                CurrencyType.Fiat => Converter.FiatToSatoshi(criteria.Amount.AmountMin, btcUnitPrice),
                _                 => throw new UnreachableException($"Unexpected type {criteria.Type}.")
            };
            var satsMax = criteria.Amount.Currency switch
            {
                CurrencyType.Sat  => (long)criteria.Amount.AmountMax,
                CurrencyType.Btc  => Converter.BitcoinToSatoshi(criteria.Amount.AmountMax / btcUnitPrice),
                CurrencyType.Fiat => Converter.FiatToSatoshi(criteria.Amount.AmountMax, btcUnitPrice),
                _                 => throw new UnreachableException($"Unexpected type {criteria.Type}.")
            };
            filter.Amount = [satsMin, satsMax];
        }

        switch (criteria.Type) {
            case CriteriaType.Default:
                var defaultCriteria = (OfferSearchCriteriaDefault)criteria;
                filter.MeansOfPayment = defaultCriteria.MeansOfPayment;
                break;
            case CriteriaType.Advanced:
                break;
            default:
                throw new UnreachableException($"Unexpected type {criteria.Type}.");
        }

        return filter;
    }

    public OfferSummaryType MapOfferType(OfferType type) => type switch
    {
        OfferType.Ask => OfferSummaryType.Sell,
        OfferType.Bid => OfferSummaryType.Buy,
        _ => throw new UnreachableException($"Unexpected value: {type}.")
    };

    public OfferSummary MapOffer(Offer offer, decimal btcUnitPrice)
    {
        var amountBtc = Converter.SatoshiToBitcoin((long)offer.Amount.ElementAt(0));
        var basePrice = amountBtc * btcUnitPrice;
        var price = offer.Type == OfferType.Ask
            ? basePrice.Increase(offer.Premium!.Value)
            : basePrice;
        OfferQuote quote = new()
        {
            AmountSat = (long)offer.Amount.ElementAt(0),
            AmountBtc = amountBtc,
            PriceFiat = Math.Round(price, 2),
            PriceFiatBase = offer.Type == OfferType.Ask ? Math.Round(basePrice, 2) : null
        };

        OfferQuote? quoteMax = null;
        if (offer.Type == OfferType.Bid) {
            var amountBtcMax = Converter.SatoshiToBitcoin((long)offer.Amount.ElementAt(1));
            var basePriceMax = amountBtcMax * btcUnitPrice;
            quoteMax = new()
            {
                AmountSat = (long)offer.Amount.ElementAt(1),
                AmountBtc = amountBtcMax,
                PriceFiat = Math.Round(basePriceMax, 2)
            };
        }

        return new OfferSummary
        {
            Id = MapToSummaryId(offer.Id),
            ReferenceId = offer.Id,
            Type = MapOfferType(offer.Type),
            PublishingDate = offer.PublishingDate,
            Quote = quote,
            QuoteMax = quoteMax,
            MeansOfPayment = offer.MeansOfPayment,
            SpreadPc = (float?)offer.Premium
        };
    }

    public string MapToSummaryId(string offerId) => $"{SUMMARY_PREFIX}{offerId}";

    public string MapToOfferId(string summaryId)
    {
        if (!summaryId.StartsWith(SUMMARY_PREFIX))
            throw new FormatException($"Summary ID must start with '{SUMMARY_PREFIX}'.");

        return summaryId.Substring(SUMMARY_PREFIX.Length);
    }
}
