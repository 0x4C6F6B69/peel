namespace Peel.Web.Models;

public enum SummaryFormat : byte
{
    Default = 0, // JSON serialized object
    Flat,        // JSON serialized object with flattened propertie
    Csv,         // Comma-separated values
}
