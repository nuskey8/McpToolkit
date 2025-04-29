using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolkit;

public record NotificationParams
{
    [JsonPropertyName("_meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Meta { get; init; }
}