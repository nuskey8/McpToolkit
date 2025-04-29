namespace McpToolkit;

[AttributeUsage(AttributeTargets.Method)]
public sealed class McpToolAttribute(string? toolName = null) : Attribute
{
    public string? ToolName { get; } = toolName;
}
