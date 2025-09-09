using Microsoft.Extensions.Diagnostics.HealthChecks;
using SharpX;

namespace Peel.Handlers;

public static class HealthHandler
{
    public static async Task<IResult> CheckHealthAsync(
        HealthCheckService service, CancellationToken cancellationToken = default)
    {
        var report = await service.CheckHealthAsync();
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            }),
            totalDuration = report.TotalDuration
        };

        return report.Status == HealthStatus.Healthy
            ? Results.Ok(report) : Results.Json(report, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    public static WebApplication MapHealthHandler(this WebApplication app)
    {
        app.MapGet("/health", CheckHealthAsync)
           .WithName("HealthCheck")
           .WithOpenApi();
        return app;
    }
}
