using Peel.Domain;

namespace Peel.Web.Models;

public record class VolatilityResponse(
    VolatilityLevel Level,
    double Score,
    string DefaultFiat
);
