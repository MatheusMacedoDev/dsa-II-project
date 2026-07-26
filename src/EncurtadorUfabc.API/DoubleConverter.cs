using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EncurtadorUfabc.API;

public class DoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDouble();

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        var formatted = value.ToString("G", CultureInfo.InvariantCulture);

        if (!formatted.Contains('.') && !formatted.Contains('E') && !formatted.Contains('e'))
            formatted += ".0";

        writer.WriteRawValue(formatted);
    }
}
