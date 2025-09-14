namespace Peel.Models;

public record class VolatilityResponse(
    VolatilityLevel Level,
    double Score,
    string DefaultFiat
);
