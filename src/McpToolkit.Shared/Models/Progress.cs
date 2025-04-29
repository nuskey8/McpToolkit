using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolkit;

public record ProgressNotificationParams : NotificationParams
{
    [JsonPropertyName("progressToken")]
    public required ProgressToken ProgressToken { get; init; }

    [JsonPropertyName("progress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Progress { get; init; }

    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Total { get; init; }
}

[StructLayout(LayoutKind.Auto)]
[JsonConverter(typeof(ProgressTokenJsonConverter))]
public readonly struct ProgressToken : IEquatable<ProgressToken>
{
    readonly ProgressTokenType type;
    readonly long numberValue;
    readonly string? stringValue;

    public ProgressTokenType Type => type;
    public bool IsValid => type is not ProgressTokenType.Invalid;

    public ProgressToken(long value)
    {
        type = ProgressTokenType.Number;
        numberValue = value;
    }

    public ProgressToken(string value)
    {
        type = ProgressTokenType.String;
        stringValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long AsNumber()
    {
        if (type is not ProgressTokenType.Number) ThrowTypeIsNot("number");
        return numberValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsString()
    {
        if (type is not ProgressTokenType.String) ThrowTypeIsNot("string");
        return stringValue!;
    }

    static void ThrowTypeIsNot(string expected)
    {
        throw new InvalidOperationException($"ProgressToken type is not a {expected}");
    }

    public override string ToString()
    {
        return type switch
        {
            ProgressTokenType.Invalid => "",
            ProgressTokenType.Number => numberValue.ToString(),
            ProgressTokenType.String => stringValue!,
            _ => "",
        };
    }

    public bool Equals(ProgressToken other)
    {
        if (type != other.type) return false;

        return type switch
        {
            ProgressTokenType.Number => numberValue == other.numberValue,
            ProgressTokenType.String => stringValue == other.stringValue,
            _ => false,
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is ProgressToken id && Equals(id);
    }

    public override int GetHashCode()
    {
        return type switch
        {
            ProgressTokenType.Number => HashCode.Combine(0, numberValue),
            ProgressTokenType.String => HashCode.Combine(1, stringValue),
            _ => 0,
        };
    }

    public static bool operator ==(ProgressToken left, ProgressToken right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ProgressToken left, ProgressToken right)
    {
        return !(left == right);
    }

    public static implicit operator ProgressToken(long value)
    {
        return new ProgressToken(value);
    }

    public static implicit operator ProgressToken(string value)
    {
        return new ProgressToken(value);
    }
}

public enum ProgressTokenType : byte
{
    Invalid,
    Number,
    String
}

public sealed class ProgressTokenJsonConverter : JsonConverter<ProgressToken>
{
    public override ProgressToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => new ProgressToken(reader.GetInt64()),
            JsonTokenType.String => new ProgressToken(reader.GetString()!),
            _ => throw new JsonException("Invalid type for ProgressToken. Expected long or string.")
        };
    }

    public override void Write(Utf8JsonWriter writer, ProgressToken value, JsonSerializerOptions options)
    {
        switch (value.Type)
        {
            case ProgressTokenType.Number:
                writer.WriteNumberValue(value.AsNumber());
                break;
            case ProgressTokenType.String:
                writer.WriteStringValue(value.AsString());
                break;
        }
    }
}
