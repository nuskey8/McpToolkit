using System.Text.Json.Serialization;

namespace McpToolkit;

public record ServerCapabilities
{
    [JsonPropertyName("experimental")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Experimental { get; init; }

    [JsonPropertyName("logging")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Logging { get; init; }

    [JsonPropertyName("completions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Completions { get; init; }

    [JsonPropertyName("prompts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ServerPromptsCapabilities? Prompts { get; init; }

    [JsonPropertyName("resources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ServerResourcesCapabilities? Resources { get; init; }

    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ServerToolsCapabilities? Tools { get; init; }
}

public record ServerPromptsCapabilities
{
    [JsonPropertyName("listChanged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ListChanged { get; init; }
}

public record ServerResourcesCapabilities
{
    [JsonPropertyName("Subscribe")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Subscribe { get; init; }

    [JsonPropertyName("listChanged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ListChanged { get; init; }
}

public record ServerToolsCapabilities
{
    [JsonPropertyName("listChanged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ListChanged { get; init; }
}