namespace Peel.Domain;

public record class VolatilityResponse(
    VolatilityLevel Level,
    double Score,
    string DefaultFiat
);
