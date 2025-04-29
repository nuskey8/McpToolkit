using System.Text.Json.Serialization;

namespace McpToolkit;

public record Annotations
{
    [JsonPropertyName("priority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Priority { get; init; }

    [JsonPropertyName("audience")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Role[]? Audience { get; init; }
}

public record ToolAnnotations
{
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    [JsonPropertyName("readOnlyHint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ReadOnlyHint { get; init; }

    [JsonPropertyName("destructiveHint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DestructiveHint { get; init; }

    [JsonPropertyName("idempotentHint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IdempotentHint { get; init; }

    [JsonPropertyName("openWorldHint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? OpenWorldHint { get; init; }
}