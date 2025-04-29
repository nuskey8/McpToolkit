using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolkit;

public record CreateMessageRequestParams : RequestParams
{
    [JsonPropertyName("messages")]
    public required SamplingMessage[] Messages { get; init; }

    [JsonPropertyName("modelPreferences")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ModelPreferences? ModelPreferences { get; init; }

    [JsonPropertyName("systemPrompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SystemPrompt { get; init; }

    [JsonPropertyName("includeContext")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ContextInclusion? IncludeContext { get; init; }

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; init; }

    [JsonPropertyName("maxTokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("stopSequences")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? StopSequences { get; init; }

    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Metadata { get; init; }
}

public record CreateMessageResult : SamplingMessage
{
    [JsonPropertyName("stopReason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StopReason { get; init; }
}

public enum ContextInclusion : byte
{
    None,
    ThisServer,
    AllServers
}

public record SamplingMessage
{
    [JsonPropertyName("role")]
    public required Role Role { get; init; }

    [JsonPropertyName("content")]
    public required Content Content { get; init; }
}

public record ModelPreferences
{
    [JsonPropertyName("hints")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ModelHint[]? Hints { get; init; }

    [JsonPropertyName("costPriority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? CostPriority { get; init; }

    [JsonPropertyName("speedPriority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SpeedPriority { get; init; }

    [JsonPropertyName("intelligencePriorityPriority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? IntelligencePriorityPriority { get; init; }
}

public record ModelHint
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }
}