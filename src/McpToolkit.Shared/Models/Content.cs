using System.Text.Json.Serialization;

namespace McpToolkit;

public record Content
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; init; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Data { get; init; }

    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MimeType { get; init; }

    [JsonPropertyName("contents")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResourceContents? Contents { get; init; }

    [JsonPropertyName("annotations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Annotations? Annotations { get; init; }

    public static implicit operator Content(string text)
    {
        return new() { Type = "text", Text = text };
    }
}