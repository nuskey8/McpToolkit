using System.Text.Json.Serialization;

namespace McpToolkit;

public record Resource
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MimeType { get; init; }

    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Size { get; init; }

    [JsonPropertyName("annotations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Annotations? Annotations { get; init; }
}

public record ResourceContents
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MimeType { get; init; }

    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; init; }

    [JsonPropertyName("blob")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Blob { get; init; }
}

public record ReadResourceRequestParams : RequestParams
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }
}

public record ReadResourceResult
{
    [JsonPropertyName("contents")]
    public required ResourceContents[] Contents { get; init; }
}

public record ListResourcesRequestParams : PaginatedRequestParams
{
}

public record ListResourcesResult : PaginatedResult
{
    [JsonPropertyName("resources")]
    public required Resource[] Resources { get; init; }
}

public record ResourceListChangedNotificationParams : NotificationParams
{
}

public record ResourceUpdatedNotificationParams : NotificationParams
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }
}