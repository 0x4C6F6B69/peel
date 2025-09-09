using System.Diagnostics;
using PeachClient.Models;
using Peel.Domain;
using SharpX;
using SharpX.Extensions;
using Xunit.Abstractions;
using Converter = Peel.Infrastructure.Converter;

public class DomainTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Get_all_summary_types_searching_in_EUR() => _ = await GetOffersSummaryAndAssertAsync(
        new OfferSearchCriteriaDefault
        {
            Type = CriteriaType.Default,
            OfferType = Peel.Domain.OfferTypeFilter.All,
            Amount = new OfferAmount(CurrencyType.Fiat, 100m, 1000m),
            MeansOfPayment = new() { ["EUR"] = ["sepa"] },
        },
        (summeries, _) => Assert.All(summeries, summary =>
        {
            if (summary.Type == OfferSummaryType.Sell) {
                Assert.InRange(summary.Quote.PriceFiat, 100m, 1000m);
            }
            else {
                Assert.True(summary.Quote.PriceFiat >= 100m && summary.QuoteMax!.PriceFiat <= 1000m,
                    userMessage: $"Summary (offer: {summary.ReferenceId}) not in range");
            }
        })
    );

    [Fact]
    public async Task Check_correctness_of_summaries_conversions_searching_in_EUR() => _ = await GetOffersSummaryAndAssertAsync(
        new OfferSearchCriteriaDefault
        {
            Type = CriteriaType.Default,
            OfferType = Peel.Domain.OfferTypeFilter.Buy,
            Amount = new OfferAmount(CurrencyType.Fiat, 100m, 1000m),
            MeansOfPayment = new() { ["EUR"] = ["sepa"] },
        },
        (summeries, btcUnitPrice) => Assert.All(summeries, summary =>
        {
            var quoteBtc = Converter.SatoshiToBitcoin((long)summary.Quote.AmountSat);
            Assert.Equal(quoteBtc, summary.Quote.AmountBtc);
            var quoteFiat = quoteBtc * btcUnitPrice;
            Assert.Equal(quoteFiat, summary.Quote.PriceFiat);
            var quoteSat = Converter.FiatToSatoshi(quoteFiat, btcUnitPrice);
            Assert.Equal(quoteSat, (long)summary.Quote.AmountSat);

            var quoteMaxBtc = Converter.SatoshiToBitcoin((long)summary.QuoteMax!.AmountSat);
            Assert.Equal(quoteMaxBtc, summary.QuoteMax.AmountBtc);
            var quoteMaxFiat = quoteMaxBtc * btcUnitPrice;
            Assert.Equal(quoteMaxFiat, summary.QuoteMax.PriceFiat);
            var quoteMaxSat = Converter.FiatToSatoshi(quoteMaxFiat, btcUnitPrice);
            Assert.Equal(quoteMaxSat, (long)summary.QuoteMax.AmountSat);
        })
);

    private async Task<List<OfferSummary>> GetOffersSummaryAndAssertAsync(
        OfferSearchCriteria criteria, Action<IEnumerable<OfferSummary>, decimal>? assert = null, bool failOnEmpty = false)
    {
        var facade = Factory.CreateFacade();

        var btcUnitPrice = (await facade.GetBtcMarketPriceAsync("EUR")).FromJustOrFail();

        output.WriteLine($"BTC price: {btcUnitPrice} EUR");

        var result = await facade.GetOffersSummaryAsync(criteria, btcUnitPrice);
        var errors = result.Errors;
        if (!errors.IsEmpty()) {
            output.WriteLine($"Completed with {errors.Count}");
            errors.ForEach(error => output.WriteLine($"  {error}"));
        }
        var summaries = result.Value;
        if (!summaries.IsEmpty()) {
            Assert.NotEmpty(summaries!);
            assert?.Invoke(summaries!, btcUnitPrice);
            output.WriteLine(ObjectDumper.Dump(summaries));
        }
        else {
            if (failOnEmpty) { Assert.Fail("No offer summeries found"); }
            else { output.WriteLine("No offer summeries found"); }
        }

        return summaries!;
    }
}
