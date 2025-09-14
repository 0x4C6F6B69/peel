using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Peel.Configuration;
using Peel.Domain;
using Peel.Infrastructure;
using Peel.Web.Models;
using SharpX;
using SharpX.Extensions;

namespace Peel.Web.Handlers;

public class OffersHandler(//ILogger<OffersHandler> logger,
    IOptions<SystemConfig> options,
    PeachFacade facade)
{
    private SystemConfig _config = options.Value;
    private static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //WriteIndented = true
    };

    public async Task<IResult> GetOffersSummaryAsync(OfferSearchCriteria filter,
            [FromQuery] SummaryFormat format = SummaryFormat.Default,
            CancellationToken cancellationToken = default)
    {
        var (error, btcUnitPrice) = await GetBtcMarketPriceAsync();
        if (error != null) { return error; }

        var result = await facade.GetOffersSummaryAsync(filter, btcUnitPrice!.Value);

        var errors = result.Errors.IsEmpty() ? null : result.Errors.Distinct().ToList();
        if (!result.IsSuccess) {
            return Results.Problem(new ProblemDetails()
            {
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "Some errors occurred while processing the request.",
                Extensions = { ["errors"] = errors ?? [] }
            });
        }

        return format switch
        {
            SummaryFormat.Default => Results.Ok(new SummaryResponse<OfferSummary>()
            {
                Summaries = result.Value ?? [],
                Errors = errors,
                DefaultFiat = _config.DefaultFiat,
                BtcUnitPrice = btcUnitPrice.Value
            }),
            SummaryFormat.Flat => Results.Ok(new SummaryResponse<OfferSummaryFlat>
            {
                Summaries = FlattenSummaries(),
                Errors = errors,
                DefaultFiat = _config.DefaultFiat,
                BtcUnitPrice = btcUnitPrice.Value
            }),
            SummaryFormat.Csv => Results.Text(
                await FlattenSummaries().ToCsvTextAsync(),
                contentType: "text/csv"),
            _ => throw new UnreachableException($"Unsupported format: {format}."),
        };

        List<OfferSummaryFlat> FlattenSummaries() => [.. result.Value.Select(s => s.Flatten(_jsonOptions))];
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
           .Produces<string>(StatusCodes.Status200OK, "text/csv")
           .Produces<SummaryResponse<OfferSummary>>(StatusCodes.Status200OK)
           .Produces<SummaryResponse<OfferSummaryFlat>>(StatusCodes.Status200OK)
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
        if (!(await facade.GetBtcMarketPriceAsync(_config.DefaultFiat)).MatchJust(out var btcUnitPrice)) {
            return (Results.BadRequest(new ErrorResult((int)HttpStatusCode.BadRequest)
            {
                Detail = $"Failed to get BTC market price in {_config.DefaultFiat}."
            }), null);
        }

        return (null, btcUnitPrice);
    }
}
