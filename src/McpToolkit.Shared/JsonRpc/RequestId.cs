using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace McpToolkit.JsonRpc;

[StructLayout(LayoutKind.Auto)]
[JsonConverter(typeof(RequestIdJsonConverter))]
public readonly struct RequestId : IEquatable<RequestId>
{
    readonly RequestIdType type;
    readonly long numberValue;
    readonly string? stringValue;

    public RequestIdType Type => type;
    public bool IsValid => type is not RequestIdType.Invalid;

    public RequestId(long value)
    {
        type = RequestIdType.Number;
        numberValue = value;
    }

    public RequestId(string value)
    {
        type = RequestIdType.String;
        stringValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long AsNumber()
    {
        if (type is not RequestIdType.Number) ThrowTypeIsNot("number");
        return numberValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsString()
    {
        if (type is not RequestIdType.String) ThrowTypeIsNot("string");
        return stringValue!;
    }

    static void ThrowTypeIsNot(string expected)
    {
        throw new InvalidOperationException($"RequestId type is not a {expected}");
    }

    public override string ToString()
    {
        return type switch
        {
            RequestIdType.Invalid => "",
            RequestIdType.Number => numberValue.ToString(),
            RequestIdType.String => stringValue!,
            _ => "",
        };
    }

    public bool Equals(RequestId other)
    {
        if (type != other.type) return false;

        return type switch
        {
            RequestIdType.Number => numberValue == other.numberValue,
            RequestIdType.String => stringValue == other.stringValue,
            _ => false,
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is RequestId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return type switch
        {
            RequestIdType.Number => HashCode.Combine(0, numberValue),
            RequestIdType.String => HashCode.Combine(1, stringValue),
            _ => 0,
        };
    }

    public static bool operator ==(RequestId left, RequestId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RequestId left, RequestId right)
    {
        return !(left == right);
    }

    public static implicit operator RequestId(long value)
    {
        return new RequestId(value);
    }

    public static implicit operator RequestId(string value)
    {
        return new RequestId(value);
    }
}

public enum RequestIdType : byte
{
    Invalid,
    Number,
    String
}