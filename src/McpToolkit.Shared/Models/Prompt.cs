using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolkit;

public record Prompt
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PromptArgument[]? Arguments { get; init; }
}

public record PromptArgument
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; init; }
}

public record PromptMessage
{
    [JsonPropertyName("role")]
    public required Role Role { get; init; }

    [JsonPropertyName("content")]
    public required Content Content { get; init; }
}

public record GetPromptRequestParams : RequestParams
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Arguments { get; init; }
}

public record GetPromptResult
{
    [JsonPropertyName("messages")]
    public required PromptMessage[] Messages { get; init; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }
}

public record ListPromptsRequestParams : PaginatedRequestParams
{
}

public record ListPromptsResult : PaginatedResult
{
    [JsonPropertyName("prompts")]
    public required Prompt[] Prompts { get; init; }
}

public record PromptListChangedNotificationParams : NotificationParams
{
}