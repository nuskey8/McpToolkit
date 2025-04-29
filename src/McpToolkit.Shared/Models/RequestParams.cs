using System.Text.Json.Serialization;

namespace McpToolkit;

public record RequestParams
{
    [JsonPropertyName("_meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RequestParamsMetadata? Meta { get; init; }
}

public record RequestParamsMetadata
{
    [JsonPropertyName("progressToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProgressToken? ProgressToken { get; init; }
}
