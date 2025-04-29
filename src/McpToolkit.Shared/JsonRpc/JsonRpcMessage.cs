using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolkit.JsonRpc;

[JsonConverter(typeof(JsonRpcMessageJsonConverter))]
public abstract record JsonRpcMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";
}

public record JsonRpcRequest : JsonRpcMessage
{
    [JsonPropertyName("id")]
    public RequestId Id { get; set; }

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("params")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Params { get; init; }
}

public record JsonRpcResponse : JsonRpcMessage
{
    [JsonPropertyName("id")]
    public required RequestId Id { get; init; }

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Result { get; init; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? Error { get; init; }
}

public record JsonRpcNotification : JsonRpcMessage
{
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("params")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Params { get; init; }
}

public record JsonRpcError
{
    [JsonPropertyName("code")]
    public required int Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; init; }
}