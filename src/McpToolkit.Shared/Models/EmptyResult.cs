using System.Text.Json.Serialization;

namespace McpToolkit;

public sealed class EmptyResult
{
    [JsonIgnore]
    public static readonly EmptyResult Instance = new();
}