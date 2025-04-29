using System.Text.Json.Serialization;

namespace McpToolkit;

public record ResourceTemplate
{
    [JsonPropertyName("uriTemplate")]
    public required string UriTemplate { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MimeType { get; init; }

    [JsonPropertyName("annotations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Annotations? Annotations { get; init; }
}

public record ListResourceTemplatesRequestParams : PaginatedRequestParams
{
}

public record ListResourceTemplatesResult : PaginatedResult
{
    [JsonPropertyName("resourceTemplates")]
    public required ResourceTemplate[] ResourceTemplates { get; init; }
}