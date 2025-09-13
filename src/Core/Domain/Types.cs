using System.Net;

namespace Peel.Domain;

#region System
public record struct ErrorInfo(string Message, Exception? Exception = null);

/// <summary>ASP.NET pipeline default error mapping based on https://datatracker.ietf.org/doc/html/rfc7807</summary>
public sealed class ErrorResult
{
    public const string Error400BadRequest = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
    public const string Error401Unauthorized = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1";
    public const string Error403Forbidden = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3";
    public const string Error404NotFound = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4";
    public const string Error409Conflict = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8";
    public const string Error412PreconditionFailed = "https://datatracker.ietf.org/doc/html/rfc7232#section-4.2";
    public const string Error500InternalServerError = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
    public const string Error502BadGatewayError = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3";
    static readonly Dictionary<int, string> _errorMap = new() {
        { 400, Error400BadRequest },
        { 401, Error401Unauthorized },
        { 403, Error403Forbidden },
        { 404, Error404NotFound },
        { 409, Error409Conflict },
        { 412, Error412PreconditionFailed },
        { 500, Error500InternalServerError },
        { 502, Error502BadGatewayError },
    };

    public ErrorResult()
    {
    }

    public ErrorResult(int statusCode)
    {
        if (!Enum.IsDefined(typeof(HttpStatusCode), statusCode)) {
            throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Invalid status code");
        }

        Type = _errorMap.GetValueOrDefault(statusCode);
        Status = statusCode;
    }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public int? Status { get; set; }

    public string? Detail { get; set; }

    public string? TraceId { get; set; }

    public List<object> Errors { get; set; } = null!;
}
#endregion

public record class VolatilityResponse(
    VolatilityLevel Level,
    double Score,
    string DefaultFiat
);

public enum VolatilityLevel : byte
{
    Undefined = 0, // Used when calculation isn't possible or meaningful
    VeryLow,       // Extremely stable price, minimal fluctuations
    Low,           // Price relatively stable, small fluctuations
    Medium,        // Moderate price fluctuations
    High,          // Significant price fluctuations
    Extreme        // Very large price swings, highly volatile
}
