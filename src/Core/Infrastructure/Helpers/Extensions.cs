using System.Text.Json;
using Peel.Domain;
using Microsoft.Extensions.Logging;
using SharpX;
using SharpX.Extensions;

namespace Peel.Infrastructure;

public static class EitherExtensions
{
    public static Either<ErrorInfo, T> LogReturn<T>(this Either<ErrorInfo, T> either, ILogger logger)
    {
        if (either.MatchLeft(out var error)) {
            _ = error.Exception == null
                    ? logger.FailWith("{Message}", Unit.Default, error.Message)
                    : logger.PanicWith("{Message}", Unit.Default, error.Exception, error.Message);
        }

        return either;
    }
}
