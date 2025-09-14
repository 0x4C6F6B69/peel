using Peel.Domain;
using Peel.Infrastructure;
using SharpX;
using SharpX.Extensions;
using PeachClient;
using PeachClient.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Peel;

public sealed class PeachFacade(PeachApiClient client,
    Mapper mapper)
{
    public async Task<Result<List<OfferSummary>>> GetOffersSummaryAsync(OfferSearchCriteria criteria,
        decimal btcUnitPrice)
    {
        // TODO: move these values to config
        const int PAGE_SIZE = 100;
        const int MAX_FAILS = 5;
        const int COOLDOWN_MS = 150;

        var filter = mapper.MapOfferFilter(criteria, btcUnitPrice);

        List<Offer> offers = [];
        List<string> errors = [];
        var pageNum = 0;
        while (true) {
            var result = await client.SearchOffersAsync(filter,
                new OfferPagination(pageNum, PAGE_SIZE), OfferSortBy.LowestPremium);
            if (result.MatchJust(out var response)) {
                if (!response.Offers.IsEmpty()) {
                    offers.AddRange(response.Offers);
                }
                if (response.Remaining == 0) {
                    break;
                }
            }
            else {
                errors.Add($"Unable to get page number {pageNum}");
                if (errors.Count == MAX_FAILS) { break; }
            }
            pageNum++;
            await Task.Delay(TimeSpan.FromMicroseconds(COOLDOWN_MS));
        }

        var summaries = offers.Select(offer => mapper.MapOffer(offer, btcUnitPrice));

        if (criteria.Amount != null) {
            summaries = enforceAmountRangeFilter(
                criteria.Amount.AmountMin, criteria.Amount.AmountMax, summaries);
        }

        if (criteria.MinSpread != null) {
            summaries = applyMinSpreadFilter(summaries, criteria.MinSpread);
        }

        if (criteria.Type == CriteriaType.Advanced) {
            var flexCriteria = (OfferSearchCriteriaAdvanced)criteria;
            summaries = applyFlexibleCriteria(flexCriteria, summaries);
        }

        return new Result<List<OfferSummary>>([.. summaries], errors);

        static IEnumerable<OfferSummary> applyFlexibleCriteria(OfferSearchCriteriaAdvanced flexCriteria,
            IEnumerable<OfferSummary> summaries)
        {
            IEnumerable<OfferSummary> result = summaries;
            if (!flexCriteria.PaymentFiat.IsEmpty()) {
                result = result.Where(summary =>
                    flexCriteria.PaymentFiat!.Any(fiat => summary.MeansOfPayment.ContainsKey(fiat))
                );
            }
            if (!flexCriteria.PaymentMethod.IsEmpty()) {
                result = result.Where(summary =>
                    flexCriteria.PaymentMethod!.Any(method =>
                        summary.MeansOfPayment.Values.Any(valueList =>
                            valueList.Contains(method)
                    )
                ));
            }

            return result;
        }

        // NOTE: it seems that the amount filter in Peach API does not work as expected
        static IEnumerable<OfferSummary> enforceAmountRangeFilter(decimal amountMin, decimal amountMax,
            IEnumerable<OfferSummary> summaries)
        {
            var unwanted = summaries.Where(s =>
                s.Type == OfferSummaryType.Buy &&
                (s.Quote.AmountSat < amountMin || s.QuoteMax.AmountSat > amountMax)
            );

            return summaries.Except(unwanted);
        }
        
        static IEnumerable<OfferSummary> applyMinSpreadFilter(IEnumerable<OfferSummary> summaries,
            float? minSpread)
        {
            var unwanted = summaries.Where(s =>
                s.SpreadPc != null && s.SpreadPc.Value < minSpread);

            return summaries.Except(unwanted);
        }
    }

    public async Task<Maybe<OfferSummary>> GetOfferSummaryAsync(string id, decimal btcUnitPrice) =>
        (await client.GetOfferAsync(mapper.MapToOfferId(id))).Map(o => mapper.MapOffer(o, btcUnitPrice));
}
