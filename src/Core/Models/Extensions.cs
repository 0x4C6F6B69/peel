using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Peel.Models;

public static class OfferSummaryExtension
{
    public static OfferSummaryFlat Flatten(this OfferSummary summary, JsonSerializerOptions jsonOptions) =>
        new()
        {
            Id = summary.Id,
            Type = summary.Type,
            ReferenceId = summary.ReferenceId,
            PublishingDate = summary.PublishingDate,
            SpreadPc = summary.SpreadPc,
            
            QuoteAmountSat = summary.Quote.AmountSat,
            QuoteAmountBtc = summary.Quote.AmountBtc,
            QuotePriceFiatBase = summary.Quote.PriceFiatBase,
            QuotePriceFiat = summary.Quote.PriceFiat,
           
            QuoteMaxAmountSat = summary.QuoteMax?.AmountSat,
            QuoteMaxAmountBtc = summary.QuoteMax?.AmountBtc,
            QuoteMaxPriceFiatBase = summary.QuoteMax?.PriceFiatBase,
            QuoteMaxPriceFiat = summary.QuoteMax?.PriceFiat,
            
            MeansOfPayment = JsonSerializer.Serialize(summary.MeansOfPayment, jsonOptions)
        };
}
