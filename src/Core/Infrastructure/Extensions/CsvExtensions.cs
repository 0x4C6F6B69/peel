using System.Globalization;
using CsvHelper;

namespace Peel.Infrastructure;

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
