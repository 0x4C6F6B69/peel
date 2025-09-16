using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Peel.Configuration;
using Peel.Infrastructure;
using Peel.Infrastructure.Types;
using Peel.Models;
using Peel.Services;
using Peel.Web.Models;
using SharpX;
using SharpX.Extensions;

namespace Peel.Web.Handlers;

public class OffersHandler(//ILogger<OffersHandler> logger,
    IOptions<SystemConfig> options,
    OfferReader facade,
    MarketAnalyzer market)
{
    private SystemConfig _config = options.Value;
    private static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //WriteIndented = true
    };

    public async Task<IResult> GetOffersSummaryAsync(OfferSearchCriteria filter,
            [FromQuery(Name = "format")] string? outputFormat = nameof(SummaryFormat.Default),
            [FromQuery(Name = "groupBy")] string? groupByField = nameof(SummaryGroupBy.None),
            CancellationToken cancellationToken = default)
    {
        // TODO: move this value to config
        const int DEFAULT_RANGE_SLICE = 50;

        if (!Enum.TryParse<SummaryFormat>(outputFormat, ignoreCase: true, out var format)) {
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { { "format", [$"Format '{outputFormat}' is not allowed."] } },
                statusCode: (int)HttpStatusCode.BadRequest
            );
        }

        if (!Enum.TryParse<SummaryGroupBy>(groupByField, ignoreCase: true, out var groupBy)) {
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { { "groupBy", [$"Group by '{groupByField}' is not allowed."] } },
                statusCode: (int)HttpStatusCode.BadRequest
            );
        }

        if (groupBy != SummaryGroupBy.None && format != SummaryFormat.Default) {
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { { "groupBy", ["Grouping is only allowed with default JSON response."] } },
                statusCode: (int)HttpStatusCode.BadRequest
            );
        }

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

        if (groupBy != SummaryGroupBy.None) {
            return groupBy switch
            {
                SummaryGroupBy.Spread => Results.Ok(new SummaryGroupResponse<OfferSummaryBySpreadGroup>()
                {
                    Groups = [.. (result.Value ?? []).GroupBySpread()],
                    Errors = errors,
                    DefaultFiat = _config.DefaultFiat,
                    BtcUnitPrice = btcUnitPrice.Value
                }),
                SummaryGroupBy.FiatPrice => Results.Ok(new SummaryGroupResponse<OfferSummaryByPriceFiatGroup>()
                {
                    Groups = [.. (result.Value ?? []).GroupByPriceFiat(DEFAULT_RANGE_SLICE)],
                    Errors = errors,
                    DefaultFiat = _config.DefaultFiat,
                    BtcUnitPrice = btcUnitPrice.Value
                }),
                _ => throw new UnreachableException($"Unsupported grouping: {groupBy}."),
            };
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
           .Produces<SummaryGroupResponse<OfferSummaryBySpreadGroup>>(StatusCodes.Status200OK)
           .Produces<SummaryGroupResponse<OfferSummaryByPriceFiatGroup>>(StatusCodes.Status200OK)
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
        if (!(await market.GetBtcMarketPriceAsync(_config.DefaultFiat)).MatchJust(out var btcUnitPrice)) {
            return (Results.BadRequest(new ErrorResult((int)HttpStatusCode.BadRequest)
            {
                Detail = $"Failed to get BTC market price in {_config.DefaultFiat}."
            }), null);
        }

        return (null, btcUnitPrice);
    }
}
