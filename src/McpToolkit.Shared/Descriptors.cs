using System.Text.Json;

namespace McpToolkit;

public sealed class ToolDescriptor
{
    public required Tool Tool { get; init; }
    public required Func<JsonElement?, CancellationToken, ValueTask<Content[]>> Handler { get; init; }
}

public sealed class PromptDescriptor
{
    public required Prompt Prompt { get; init; }
    public required Func<JsonElement?, CancellationToken, ValueTask<PromptMessage[]>> Handler { get; init; }
}

public sealed class ResourceDescriptor
{
    public required Resource Resource { get; init; }
    public required Func<string, CancellationToken, ValueTask<ResourceContents[]>> Handler { get; init; }
}

public sealed class ResourceTemplateDescriptor
{
    public required ResourceTemplate ResourceTemplate { get; init; }
    public required Func<string, CancellationToken, ValueTask<ResourceContents[]>> Handler { get; init; }
}