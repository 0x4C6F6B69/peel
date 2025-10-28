namespace Peel.Models;

public enum VolatilityLevel : byte
{
    Undefined = 0, // Used when calculation isn't possible or meaningful
    VeryLow,       // Extremely stable price, minimal fluctuations
    Low,           // Price relatively stable, small fluctuations
    Medium,        // Moderate price fluctuations
    High,          // Significant price fluctuations
    Extreme        // Very large price swings, highly volatile
}
