using System.Text.Json.Serialization;

namespace McpToolkit;

public record InitializeRequestParams : RequestParams
{
    [JsonPropertyName("protocolVersion")]
    public required string ProtocolVersion { get; init; }

    [JsonPropertyName("capabilities")]
    public required ClientCapabilities Capabilities { get; init; }

    [JsonPropertyName("clientInfo")]
    public required Implementation ClientInfo { get; init; }
}

public record InitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public required string ProtocolVersion { get; init; }

    [JsonPropertyName("capabilities")]
    public required ServerCapabilities Capabilities { get; init; }

    [JsonPropertyName("serverInfo")]
    public required Implementation ServerInfo { get; init; }

    [JsonPropertyName("instructions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Instructions { get; init; }
}

public record InitializedNotificationParams : NotificationParams
{
}

public record Implementation
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}