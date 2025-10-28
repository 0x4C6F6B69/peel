using System.Net;
using FsCheck;
using Microsoft.Extensions.Options;
using Peel.Configuration;
using Peel.Infrastructure.Types;
using Peel.Models;
using Peel.Services;

namespace Peel.Web.Handlers;

public static class MarketHandler
{
    public static async Task<IResult> GetVolatilittAsync(
        //LoggerFactory loggerFactory,
        IOptions<SystemConfig> options,
        MarketAnalyzer market,
        float hours,
        CancellationToken cancellationToken = default)
    {
        //var logger = loggerFactory.CreateLogger(typeof(MarketHandler));
        var config = options.Value;

        if (!(await market.ComputeVolatilityAsync(config.DefaultFiat, hours)).MatchJust(out var info)) {
            return Results.BadRequest(new ErrorResult((int)HttpStatusCode.BadRequest)
            {
                Detail = "Failed to compute market volatility."
            });
        }

        return Results.Ok(new VolatilityResponse(Level: info.Item1, Score: info.Item2, config.DefaultFiat));
    }

    public static async Task<IResult> GetBtcMarketPriceAsync(IOptions<SystemConfig> options,
        MarketAnalyzer market)
    {
        var config = options.Value;

        if (!(await market.GetBtcMarketPriceAsync(config.DefaultFiat)).MatchJust(out var btcUnitPrice)) {
            return Results.BadRequest(new ErrorResult((int)HttpStatusCode.BadRequest)
            {
                Detail = $"Failed to get BTC market price in {config.DefaultFiat}."
            });
        }

        return Results.Ok(new BtcPriceResponse(btcUnitPrice, config.DefaultFiat));
    }

    public static WebApplication MapMarketHandler(this WebApplication app)
    {
        app.MapGet("/market/volatility/{hours}", GetVolatilittAsync)
           .WithName("VolatilityGet")
           .WithOpenApi()
           .Produces<VolatilityResponse>();

        app.MapGet("/market/price/BTC", GetBtcMarketPriceAsync)
           .WithName("BtcMarketPriceGet")
           .WithOpenApi()
           .Produces<BtcPriceResponse>();

        return app;
    }
}
