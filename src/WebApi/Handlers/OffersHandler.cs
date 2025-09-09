using System.Net;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Peel;
using Peel.Configuration;
using Peel.Domain;
using Peel.Infrastructure;
using Peel.Models;
using SharpX;
using SharpX.Extensions;

namespace Peel.Handlers;

public class OffersHandler(//ILogger<OffersHandler> logger,
    IOptions<SystemConfig> options,
    PeachFacade facade)
{
    private SystemConfig config = options.Value;

    public async Task<IResult> GetOffersSummaryAsync(OfferSearchCriteria filter,
        CancellationToken cancellationToken = default)
    {
        var (error, btcUnitPrice) = await GetBtcMarketPriceAsync();
        if (error != null) { return error; }

        var result = await facade.GetOffersSummaryAsync(filter, btcUnitPrice!.Value);

        SummaryResponse response = new()
        {
            Summaries = result.Value ?? [],
            Errors = result.Errors.IsEmpty() ? null : [.. result.Errors.Distinct()],
            BtcUnitPrice = btcUnitPrice.Value
        };

        return response.Errors.IsEmpty()
            ? Results.Ok(response)
            : Results.BadRequest(new ErrorResult((int)HttpStatusCode.BadRequest)
            {
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "Some errors occurred while processing the request.",
                Errors = [.. response.Errors!]
            });
    }

    public async Task<IResult> GetSingleOfferSummaryAsync(string id)
    {
        var (error, btcUnitPrice) = await GetBtcMarketPriceAsync();
        if (error != null) { return error; }

        var result = await facade.GetOfferSummaryAsync(id, btcUnitPrice!.Value);

        return result.MatchJust(out var summary)
            ? Results.Ok(summary)
            : Results.NotFound(new ErrorResult((int)HttpStatusCode.NotFound)
            {
                Status = (int)HttpStatusCode.NotFound,
                Detail = $"Offer Summary with id '{id}' not found."
            });
    }

    public WebApplication Map(WebApplication app)
    {
        app.MapPost("/offers/summary", GetOffersSummaryAsync)
           .WithName("OffersSummaryGet")
           .WithOpenApi()
           .Accepts<OfferSearchCriteria>("application/json")
           .Produces<List<OfferSummary>>(StatusCodes.Status200OK)
           .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
           .Produces<ErrorResult>(StatusCodes.Status400BadRequest);

        app.MapGet("/offers/summary/{id}", GetSingleOfferSummaryAsync)
           .WithName("SingleOfferSummaryGet")
           .WithOpenApi()
           .Produces<OfferSummary>(StatusCodes.Status200OK)
           .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
           .Produces<ErrorResult>(StatusCodes.Status400BadRequest);

        return app;
    }

    private async Task<(IResult?, decimal?)> GetBtcMarketPriceAsync()
    {
        if (!(await facade.GetBtcMarketPriceAsync(config.DefaultFiat)).MatchJust(out var btcUnitPrice)) {
            return (Results.BadRequest(new ErrorResult((int)HttpStatusCode.BadRequest)
            {
                Detail = $"Failed to get BTC market price in {config.DefaultFiat}."
            }), null);
        }

        return (null, btcUnitPrice);
    }
}
