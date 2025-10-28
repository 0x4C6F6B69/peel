using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Peel.Models;
using SharpX.Extensions;

namespace Peel;

public class OfferSearchCriteriaConverter : JsonConverter<OfferSearchCriteria>
{
    public override OfferSearchCriteria Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeProp)) {
            throw new JsonException("Missing required 'type' property.");
        }

        var typeValue = typeProp.GetString()?.ToLowerInvariant();

        if (typeValue.IsEmpty()) {
            throw new JsonException("The 'type' property cannot be empty.");
        }

        return typeValue switch
        {
            "default" =>
                JsonSerializer.Deserialize<OfferSearchCriteriaDefault>(root.GetRawText(), options)
                ?? throw new JsonException($"Failed to deserialize as {nameof(OfferSearchCriteriaDefault)}."),

            "advanced" =>
                JsonSerializer.Deserialize<OfferSearchCriteriaAdvanced>(root.GetRawText(), options)
                ?? throw new JsonException($"Failed to deserialize as {nameof(OfferSearchCriteriaAdvanced)}."),

            _ => throw new JsonException($"Unexpected type discriminator: '{typeValue}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, OfferSearchCriteria value, JsonSerializerOptions options)
    {
        throw new UnreachableException();
    }
}
