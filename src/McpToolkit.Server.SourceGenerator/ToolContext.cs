using Microsoft.CodeAnalysis;

namespace McpToolkit.Server.SourceGenerator;

public record ToolContext
{
    public ToolMetadata?[] MetadataList { get; set; } = [];
    public IgnoreEquality<ITypeSymbol?> TypeSymbol { get; set; }
    public required DiagnosticReporter DiagnosticReporter { get; init; }
    public required SemanticModel Model { get; init; }
}

public record ToolMetadata
{
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required string? ReturnType { get; init; }
    public required string InvocationSymbol { get; init; }
    public required EquatableArray<ToolParameter> Parameters { get; init; }
    public required bool IsAsync { get; init; }
    public required IgnoreEquality<Location> NameSyntaxLocation { get; init; }
}

public record ToolParameter
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required bool IsCancellationToken { get; init; }
    public required string JsonSchemaType { get; init; }
    public required string? JsonSchemaFormat { get; init; }
    public required string? JsonSchemaDescription { get; init; }
}
