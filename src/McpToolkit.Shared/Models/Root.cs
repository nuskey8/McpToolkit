using System.Text.Json.Serialization;

namespace McpToolkit;

public record Root
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }
}

public record ListRootsRequestParams : RequestParams
{
}

public record ListRootsResult
{
    [JsonPropertyName("roots")]
    public required Root[] Roots { get; init; }
}

public record RootsListChangedNotificationParams : NotificationParams
{
}