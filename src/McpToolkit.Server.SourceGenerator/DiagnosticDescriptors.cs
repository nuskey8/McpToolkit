using Microsoft.CodeAnalysis;

namespace McpToolkit.Server.SourceGenerator;

public sealed class DiagnosticReporter
{
    List<Diagnostic>? diagnostics;

    public bool HasDiagnostics => diagnostics != null && diagnostics.Count != 0;

    public void ReportDiagnostic(DiagnosticDescriptor diagnosticDescriptor, Location location, params object?[]? messageArgs)
    {
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, location, messageArgs);
        diagnostics ??= [];
        diagnostics.Add(diagnostic);
    }

    public void ReportToContext(SourceProductionContext context)
    {
        if (diagnostics != null)
        {
            foreach (var item in diagnostics)
            {
                context.ReportDiagnostic(item);
            }
        }
    }
}

public static class DiagnosticDescriptors
{
    const string Category = "McpToolkitSourceGeneration";

    public static void ReportDiagnostic(this SourceProductionContext context, DiagnosticDescriptor diagnosticDescriptor, Location location, params object?[]? messageArgs)
    {
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, location, messageArgs);
        context.ReportDiagnostic(diagnostic);
    }

    public static DiagnosticDescriptor Create(int id, string message)
    {
        return Create(id, message, message);
    }

    public static DiagnosticDescriptor Create(int id, string title, string messageFormat)
    {
        return new DiagnosticDescriptor(
            id: "MCP" + id.ToString("000"),
            title: title,
            messageFormat: messageFormat,
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }

    public static DiagnosticDescriptor MustBePartial { get; } = Create(
        1,
        "McpToolProvider type must be partial.");

    public static DiagnosticDescriptor StaticOrAbstractNotAllowed { get; } = Create(
        2,
        "McpToolProvider type does not allow static or abstract class.");

    public static DiagnosticDescriptor DefinedInOtherProject { get; } = Create(
        3,
        "McpToolkit cannot register type/method in another project outside the SourceGenerator referenced project.");

    public static DiagnosticDescriptor ToolNameMustBeStringLiteral { get; } = Create(
        4,
        "Tools.Add 'name' argument must be string literal."
    );

    public static DiagnosticDescriptor ToolDescriptionMustBeNullOrStringLiteral { get; } = Create(
        5,
        "Tools.Add 'description' argument must be null or string literal."
    );


    public static DiagnosticDescriptor DuplicateToolName { get; } = Create(
        6,
        "Tool name is duplicated.",
        "Tool name '{0}' is duplicated."
    );

    public static readonly DiagnosticDescriptor ToolParameterTypeIsNotSupported = Create(
        7,
        "Parameter type is not supported",
        "The parameter type '{0}' is not supported in tool"
    );
}
