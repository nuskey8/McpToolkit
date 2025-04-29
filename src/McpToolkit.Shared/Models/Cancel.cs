using System.Text.Json.Serialization;
using McpToolkit.JsonRpc;

namespace McpToolkit;

public record CancelledNotificationParams : NotificationParams
{
    [JsonPropertyName("requestId")]
    public required RequestId RequestId { get; init; }

    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; init; }
}