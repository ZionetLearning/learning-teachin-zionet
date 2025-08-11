using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Accessor.DB;

public sealed class UtcDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTimeOffset.Parse(reader.GetString()!, null, DateTimeStyles.RoundtripKind);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
}
