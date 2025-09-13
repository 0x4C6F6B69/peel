using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Peel.Domain;
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

public static class CsvExtensions
{
    /// <summary>
    /// Converts a list of records to a CSV string.
    /// </summary>
    public static async Task<string> ToCsvTextAsync<T>(this IEnumerable<T> records)
    {
        await using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(records);
        await writer.FlushAsync();

        return writer.ToString();
    }
}
