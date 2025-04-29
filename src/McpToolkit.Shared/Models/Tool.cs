using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolkit;

public record Tool
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    [JsonPropertyName("inputSchema")]
    public required JsonElement InputSchema { get; init; }

    [JsonPropertyName("annotation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolAnnotations? Annotations { get; init; }
}

public record CallToolRequestParams : RequestParams
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Arguments { get; init; }
}

public record CallToolResult
{
    [JsonPropertyName("content")]
    public required Content[] Content { get; init; }

    [JsonPropertyName("isError")]
    public bool IsError { get; init; }
}

public record ListToolsRequestParams : PaginatedRequestParams
{
}

public record ListToolsResult : PaginatedResult
{
    [JsonPropertyName("tools")]
    public required Tool[] Tools { get; init; }
}

public record ToolListChangedNotificationParams : NotificationParams
{
}