using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolkit;

#if NET9_0_OR_GREATER
[JsonConverter(typeof(JsonStringEnumConverter<LoggingLevel>))]
#else
[JsonConverter(typeof(PolyfillJsonStringEnumConverter<LoggingLevel>))]
#endif
public enum LoggingLevel
{
    [JsonStringEnumMemberName("debug")]
    Debug,

    [JsonStringEnumMemberName("info")]
    Info,

    [JsonStringEnumMemberName("notice")]
    Notice,

    [JsonStringEnumMemberName("warning")]
    Warning,

    [JsonStringEnumMemberName("error")]
    Error,

    [JsonStringEnumMemberName("critical")]
    Critical,

    [JsonStringEnumMemberName("alert")]
    Alert,

    [JsonStringEnumMemberName("emergency")]
    Emergency
}

public record SetLevelRequestParams : RequestParams
{
    [JsonPropertyName("level")]
    public required LoggingLevel Level { get; init; }
}

public record LoggingMessageNotificationParams : RequestParams
{
    [JsonPropertyName("level")]
    public required LoggingLevel Level { get; init; }

    [JsonPropertyName("logger")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Logger { get; init; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Data { get; init; }
}