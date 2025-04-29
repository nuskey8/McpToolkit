using System.Text.Json.Serialization;

namespace McpToolkit;

public abstract record PaginatedRequestParams : RequestParams
{
    [JsonPropertyName("cursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Cursor { get; init; }
}

public abstract record PaginatedResult
{
    [JsonPropertyName("nextCursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NextCursor { get; init; }
}