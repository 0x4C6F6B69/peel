using System.Net;
using Peel;
using Peel.Domain;
using Peel.Infrastructure;
using Peel.Market;
using SharpX;

namespace Peel.Handlers;

public static class MarketHandler
{
    public static async Task<IResult> GetVolatilittAsync(ILoggerFactory loggerFactory,
        HttpContext context,
        MarketAnalyzer market,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger(typeof(OffersHandler));

        var rawRequest = (await context.GetRequestAsync<VolatiltiyRequest>()).LogReturn(logger);
        if (rawRequest.MatchLeft(out var error)) {
            return Results.BadRequest(new ErrorResult((int)HttpStatusCode.BadRequest)
            {
                Detail = error.Message
            });
        }
        var request = rawRequest.FromRight();

        if (!(await market.ComputeVolatilityAsync(request.FiatCurrency, request.Hours)).MatchJust(out var info)) {
            return Results.BadRequest(new ErrorResult((int)HttpStatusCode.BadRequest)
            {
                Detail = "Failed to compute market volatility."
            });
        }

        return Results.Ok(new VolatilityResponse(Level: info.Item1, Score: info.Item2));
    }

    public static WebApplication MapMarketHandler(this WebApplication app)
    {
        app.MapPost("/market/volatility", GetVolatilittAsync)
           .WithName("VolatilityGet")
           .WithOpenApi()
           .Accepts<VolatiltiyRequest>("application/json")
           .Produces<VolatilityResponse>();

        return app;
    }
}
