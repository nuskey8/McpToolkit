using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolkit.JsonRpc;

public sealed class RequestIdJsonConverter : JsonConverter<RequestId>
{
    public override RequestId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => new RequestId(reader.GetInt64()),
            JsonTokenType.String => new RequestId(reader.GetString()!),
            _ => throw new JsonException("Invalid type for RequestId. Expected long or string.")
        };
    }

    public override void Write(Utf8JsonWriter writer, RequestId value, JsonSerializerOptions options)
    {
        switch (value.Type)
        {
            case RequestIdType.Number:
                writer.WriteNumberValue(value.AsNumber());
                break;
            case RequestIdType.String:
                writer.WriteStringValue(value.AsString());
                break;
        }
    }
}
