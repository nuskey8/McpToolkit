using System.Text.Json.Serialization;

namespace McpToolkit;

#if NET9_0_OR_GREATER
[JsonConverter(typeof(JsonStringEnumConverter<Role>))]
#else
[JsonConverter(typeof(PolyfillJsonStringEnumConverter<Role>))]
#endif
public enum Role : byte
{
    [JsonStringEnumMemberName("user")]
    User,
    [JsonStringEnumMemberName("assistant")]
    Assistant
}