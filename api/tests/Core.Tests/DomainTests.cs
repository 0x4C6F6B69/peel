using Peel.Models;
using SharpX;
using SharpX.Extensions;
using Xunit.Abstractions;
using Converter = Peel.Infrastructure.Converter;

public class Domain_tests(ITestOutputHelper output)
{

    [Fact]
    public async Task Get_all_summary_types_searching_in_EUR() => _ = await GetOffersSummaryAndAssertAsync(
        new OfferSearchCriteriaDefault
        {
            Type = CriteriaType.Default,
            OfferType = Peel.Models.OfferTypeFilter.All,
            Amount = new OfferAmount(CurrencyType.Fiat, 100m, 1000m),
            MeansOfPayment = new() { ["EUR"] = ["sepa"] },
        },
        (summeries, _) => Assert.All(summeries, summary =>
        {
            if (summary.Type == OfferSummaryType.Sell) {
                Assert.InRange(summary.Quote.PriceFiat, 100m, 1000m);
            }
            //else {
            //    Assert.True(summary.Quote.PriceFiat >= 100m && summary.QuoteMax!.PriceFiat <= 1000m,
            //        userMessage: $"Summary (offer: {summary.ReferenceId}) not in range");
            //}
        })
    );

    [Fact]
    public async Task Check_correctness_of_summaries_conversions_searching_in_EUR()
    {
        const long SAT_TOLERANCE = 8;

        _ = await GetOffersSummaryAndAssertAsync(
            new OfferSearchCriteriaDefault
            {
                Type = CriteriaType.Default,
                OfferType = Peel.Models.OfferTypeFilter.Buy,
                Amount = new OfferAmount(CurrencyType.Fiat, 100m, 1000m),
                //MeansOfPayment = new() { ["EUR"] = ["sepa"] },
            },
            (summeries, btcUnitPrice) =>
            {
                Assert.All(summeries, summary =>
                {
                    var quoteBtc = Converter.SatoshiToBitcoin((long)summary.Quote.AmountSat);
                    Assert.Equal(quoteBtc, summary.Quote.AmountBtc);
                    var quoteFiat = Math.Round(quoteBtc * btcUnitPrice, 2);
                    Assert.Equal(quoteFiat, summary.Quote.PriceFiat);
                    var quoteSat = Converter.FiatToSatoshi(quoteFiat, btcUnitPrice);
                    AssertEx.TolerantEqual(quoteSat, (long)summary.Quote.AmountSat, SAT_TOLERANCE);

                    //var quoteMaxBtc = Converter.SatoshiToBitcoin((long)summary.QuoteMax!.AmountSat);
                    //Assert.Equal(quoteMaxBtc, summary.QuoteMax.AmountBtc);
                    //var quoteMaxFiat = Math.Round(quoteMaxBtc * btcUnitPrice, 2);
                    //Assert.Equal(quoteMaxFiat, summary.QuoteMax.PriceFiat);
                    //var quoteMaxSat = Converter.FiatToSatoshi(quoteMaxFiat, btcUnitPrice);
                    //AssertEx.TolerantEqual(quoteMaxSat, (long)summary.QuoteMax.AmountSat, SAT_TOLERANCE);
                });
            }
        );
    }

    private async Task<List<OfferSummary>> GetOffersSummaryAndAssertAsync(
        OfferSearchCriteria criteria, Action<IEnumerable<OfferSummary>, decimal>? assert = null, bool failOnEmpty = false)
    {
        var facade = Factory.CreateFacade();
        var market = Factory.CreateMarket();

        var btcUnitPrice = (await market.GetBtcMarketPriceAsync("EUR")).FromJustOrFail();

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
