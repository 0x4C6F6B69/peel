using System.Text.Json;
using System.Text.Json.Serialization;
using Peel.Domain;
using Microsoft.Extensions.Logging;
using SharpX;
using SharpX.Extensions;

namespace Peel;

public static class HttpContextExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static async Task<Either<ErrorInfo, T>> GetRequestAsync<T>(this HttpContext context)
        where T : class
    {
        T? requestBody = null;
        Exception? exception = null;

        try {
            // Enable buffering to allow multiple reads of the request body
            context.Request.EnableBuffering();

            requestBody = await JsonSerializer.DeserializeAsync<T>(
                context.Request.Body, _jsonOptions);
        }
        catch (Exception ex) {
            exception = ex;
        }

        return exception == null && requestBody != null
            ? Either.Right<ErrorInfo, T>(requestBody)
            : Either.Left<ErrorInfo, T>(new("Failed to deserialize the request body", exception));
    }
}
