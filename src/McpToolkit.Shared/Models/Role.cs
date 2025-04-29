namespace McpToolkit;

public enum Role : byte
{
    [JsonStringEnumMemberName("user")]
    User,
    [JsonStringEnumMemberName("assistant")]
    Assistant
}