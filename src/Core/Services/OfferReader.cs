using Peel.Models;
using Peel.Infrastructure;
using SharpX;
using SharpX.Extensions;
using PeachClient;
using PeachClient.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Peel.Infrastructure.Types;

namespace Peel.Services;

public sealed class OfferReader(PeachApiClient client,
    Mapper mapper)
{
    public async Task<Result<List<OfferSummary>>> GetOffersSummaryAsync(OfferSearchCriteria criteria,
        decimal btcUnitPrice)
    {
        // TODO: move these values to config
        const int PAGE_SIZE = 100;
        const int MAX_FAILS = 5;
        const int COOLDOWN_MS = 150;

        List<Offer> offers = [];
        List<string> errors = [];
        var pageNum = 0;
        var sellExhausted = false;
        var buyExhausted = false;

        while (!sellExhausted || !buyExhausted) {
            var response = await SearchOffersAsync(pageNum, applyAllFitlers);
            if (response != null) {
                if (!response.Offers.IsEmpty()) {
                    offers.AddRange(response.Offers);
                }
                sellExhausted = response.SellExhausted;
                buyExhausted = response.BuyExhausted;
            }
            else {
                errors.Add($"Unable to get page number {pageNum}");
                if (errors.Count == MAX_FAILS) { break; }
            }
            pageNum++;
            await Task.Delay(TimeSpan.FromMicroseconds(COOLDOWN_MS));
        }

        var summaries = offers.Select(offer => mapper.MapOffer(offer, btcUnitPrice));

        if (criteria.Type == CriteriaType.Advanced) {
            var flexCriteria = (OfferSearchCriteriaAdvanced)criteria;
            summaries = applyFlexibleCriteria(flexCriteria, summaries);
        }

        return new Result<List<OfferSummary>>([.. summaries], errors);

        // Last Peach API update does not support filters, so we need to apply here
        IEnumerable<Offer> applyAllFitlers(IEnumerable<Offer> offers)
        {
            var filteredOffers = offers;
            if (criteria.Amount != null) {
                filteredOffers = filteredOffers.Where(
                    o => o.Amount >= criteria.Amount.GetMinInSatoshi(btcUnitPrice) &&
                         o.Amount <= criteria.Amount.GetMaxInSatoshi(btcUnitPrice));
            }
            if (criteria.MinSpread != null) {
                filteredOffers = filteredOffers.Where(o => o.Premium >= (decimal)criteria.MinSpread.Value);
            }
            if (criteria.MaxSpread != null) {
                filteredOffers = filteredOffers.Where(o => o.Premium <= (decimal)criteria.MaxSpread.Value);
            }
            if (criteria.MinReputation != null) {
                filteredOffers = filteredOffers.Where(o => o.User.UserRating >= criteria.MinReputation.Value);
            }

            return filteredOffers;
        }
         

        async Task<CombinedResponse?> SearchOffersAsync(int pageNum,
            Func<IEnumerable<Offer>, IEnumerable<Offer>> filter)
        {
            OfferResponse? sellResponse = null;
            if (criteria.HasSellFilter() || criteria.HasAllFilter()) {
                sellResponse = await client.SearchOffersAsync(
                    PeachClient.Models.OfferTypeFilter.Sell, new OfferPagination(pageNum, PAGE_SIZE), OfferSortBy.LowestPremium);
                if (criteria.HasSellFilter()) return ToCombinedResponse(sellResponse, isSell: true);
            }

            OfferResponse? buyResponse = null;
            if (criteria.HasBuyFilter() || criteria.HasAllFilter()) {
                buyResponse = await client.SearchOffersAsync(
                    PeachClient.Models.OfferTypeFilter.Buy, new OfferPagination(pageNum, PAGE_SIZE), OfferSortBy.LowestPremium);
                if (criteria.HasBuyFilter()) return ToCombinedResponse(buyResponse, isSell: false);
            }

            if (sellResponse == null &&  buyResponse == null) {
                errors.Add("Failed to get both sell and buy offers");
                return null;
            }

            // If both kind are requested and one is null than we return the other
            if (sellResponse == null) {
                errors.Add("Failed to get sell offers");
                return ToCombinedResponse(buyResponse, isSell: false); ;
            }
            if (buyResponse == null) {
                errors.Add("Failed to get buy offers");
                return ToCombinedResponse(sellResponse, isSell: true); ;
            }

            // Since Peach API does not provide a mean to get all offers we neeed to merge results
            return new CombinedResponse(
                filter(sellResponse.Offers.Concat(buyResponse.Offers)).ToList(),
                sellResponse.Total + buyResponse.Total,
                SellExhausted: sellResponse.Remaining == 0,
                BuyExhausted: buyResponse.Remaining == 0);

            CombinedResponse? ToCombinedResponse(OfferResponse? resp, bool isSell)
            {
                if (resp == null) return null;

                return new(filter(resp.Offers).ToList(), resp.Total,
                    SellExhausted: isSell ? resp.Remaining == 0 : true,
                    BuyExhausted: !isSell ? resp.Remaining == 0 : true);
            }
        }

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
    }

    public async Task<Maybe<OfferSummary>> GetOfferSummaryAsync(string id, decimal btcUnitPrice) =>
        (await client.GetOfferAsync(mapper.MapToOfferId(id))).Map(o => mapper.MapOffer(o, btcUnitPrice));
}
