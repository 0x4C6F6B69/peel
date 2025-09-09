using Microsoft.Extensions.Diagnostics.HealthChecks;
using PeachClient;
using SharpX;
using SharpX.Extensions;

namespace Peel;

public class PeachHealthCheck(ILogger<PeachHealthCheck> logger,
    PeachApiClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) {
            return HealthCheckResult.Unhealthy("Peach health check canceled");
        }

        var result = await client.GetSystemStatusAsync();

        return (result) switch
        {
            { Tag: MaybeType.Just } => result.FromJust()!.Status.EqualsIgnoreCase("online")
                ? HealthCheckResult.Healthy("Peach service is running")
                : HealthCheckResult.Unhealthy("Peach service is down"),
            _ => HealthCheckResult.Unhealthy("Failed to determine Peach service status"),
        };
    }
}
