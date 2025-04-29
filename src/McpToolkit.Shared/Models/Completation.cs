using System.Text.Json.Serialization;

namespace McpToolkit;

public record CompleteRequestParams : RequestParams
{
    [JsonPropertyName("argument")]
    public required CompleteArgument Argument { get; init; }

    [JsonPropertyName("ref")]
    public required Reference Ref { get; init; }
}

public record CompleteResult
{
    [JsonPropertyName("completion")]
    public required Completion Completion { get; init; }
}

public record CompleteArgument
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

public record Reference
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Uri { get; init; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }
}

public record Completion
{
    [JsonPropertyName("values")]
    public required string[] Values { get; init; }

    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Total { get; init; }

    [JsonPropertyName("hasMore")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HasMore { get; init; }
}