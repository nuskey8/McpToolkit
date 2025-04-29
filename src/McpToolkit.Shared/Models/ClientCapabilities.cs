using System.Text.Json.Serialization;

namespace McpToolkit;

public record ClientCapabilities
{
    [JsonPropertyName("experimental")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Experimental { get; init; }

    [JsonPropertyName("roots")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ClientCapabilitiesRoots? Roots { get; init; }

    [JsonPropertyName("sampling")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Sampling { get; init; }
}

public record ClientCapabilitiesRoots
{
    [JsonPropertyName("listChanged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ListChanged { get; init; }
}
