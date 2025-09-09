using System.Diagnostics;
using PeachClient.Models;
using Peel.Domain;
using SharpX;
using SharpX.Extensions;
using Xunit.Abstractions;

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
        summeries => Assert.All(summeries, s =>
        {
            if (s.Type == OfferSummaryType.Sell) {
                Assert.InRange(s.Quote.PriceFiat, 100m, 1000m);
            }
            else {
                Assert.True(s.Quote.PriceFiat >= 100m && s.QuoteMax!.PriceFiat <= 1000m,
                    userMessage: $"Summary (offer: {s.ReferenceId}) not in range");
            }
        })
    );

    private async Task<List<OfferSummary>> GetOffersSummaryAndAssertAsync(
        OfferSearchCriteria criteria, Action<IEnumerable<OfferSummary>>? assert = null, bool failOnEmpty = false)
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
            assert?.Invoke(summaries!);
            output.WriteLine(ObjectDumper.Dump(summaries));
        }
        else {
            if (failOnEmpty) { Assert.Fail("No offer summeries found"); }
            else { output.WriteLine("No offer summeries found"); }
        }

        return summaries!;
    }
}
