using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace McpToolkit.Server.SourceGenerator;

internal static class Parser
{
    static bool TryGetJsonSchemaType(ITypeSymbol symbol, [NotNullWhen(true)] out string? jsonSchemaType, out string? format)
    {
        format = null;

        switch (symbol.SpecialType)
        {
            case SpecialType.System_Boolean:
                jsonSchemaType = "boolean";
                return true;
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                jsonSchemaType = "integer";
                return true;
            case SpecialType.System_Decimal:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
                jsonSchemaType = "number";
                return true;
            case SpecialType.System_DateTime:
                jsonSchemaType = "string";
                format = "date-time";
                return true;
            case SpecialType.System_Char:
            case SpecialType.System_String:
            case SpecialType.System_Enum:
                jsonSchemaType = "string";
                return true;
        }

        jsonSchemaType = null;
        return false;
    }

    public static ToolMetadata? ParseFromLambda(string toolName, string? toolDescription, ParenthesizedLambdaExpressionSyntax lambda, Location nameSyntaxLocation, SemanticModel semanticModel, DiagnosticReporter reporter)
    {
        var parameters = new ToolParameter[lambda.ParameterList.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++)
        {
            var x = lambda.ParameterList.Parameters[i];
            var type = semanticModel.GetTypeInfo(x.Type!);
            var typeName = type.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            string? jsonSchemaType = null;
            string? jsonSchemaFormat = null;
            bool isCancellationToken = false;
            if (typeName == "global::System.Threading.CancellationToken")
            {
                isCancellationToken = true;
            }
            else if (!TryGetJsonSchemaType(type.Type, out jsonSchemaType, out jsonSchemaFormat))
            {
                reporter.ReportDiagnostic(DiagnosticDescriptors.ToolParameterTypeIsNotSupported,
                    x.Type!.GetLocation(),
                    typeName);
                return null;
            }

            parameters[i] = new ToolParameter
            {
                Name = x.Identifier.Text,
                Type = type.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                JsonSchemaType = jsonSchemaType!,
                JsonSchemaFormat = jsonSchemaFormat,
                JsonSchemaDescription = null,
                IsCancellationToken = isCancellationToken,
            };
        }

        // check return type
        var lambdaType = semanticModel.GetTypeInfo(lambda).Type as INamedTypeSymbol;
        var lambdaReturnType = lambdaType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Contains("global::System.Func")
            ? lambdaType.TypeArguments.Last()
            : null;

        return new ToolMetadata
        {
            Name = toolName,
            Description = toolDescription,
            InvocationSymbol = $"(({lambdaType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})action)",
            ReturnType = new(lambdaReturnType!),
            ReturnTypeName = lambdaReturnType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Parameters = new(parameters),
            IsAsync = lambda.Modifiers.Any(x => x.IsKind(SyntaxKind.AsyncKeyword)),
            NameSyntaxLocation = new(nameSyntaxLocation),
        };
    }

    public static ToolMetadata? ParseFromMethod(string toolName, string? toolDescription, IMethodSymbol methodSymbol, SyntaxNode invocationNode, Location nameSyntaxLocation, DiagnosticReporter reporter)
    {
        if (methodSymbol.DeclaringSyntaxReferences.Length == 0)
        {
            reporter.ReportDiagnostic(DiagnosticDescriptors.DefinedInOtherProject, invocationNode.GetLocation());
            return null;
        }

        return ParseFromMethodCore(toolName, toolDescription, methodSymbol, nameSyntaxLocation, reporter);
    }

    static ToolMetadata? ParseFromMethodCore(string toolName, string? toolDescription, IMethodSymbol methodSymbol, Location nameSyntaxLocation, DiagnosticReporter reporter)
    {
        // description
        var docComment = methodSymbol.DeclaringSyntaxReferences[0].GetSyntax().GetDocumentationCommentTriviaSyntax();
        string? summary = null;
        Dictionary<string, string>? parameterDescriptions = null;
        if (docComment != null)
        {
            summary = docComment.GetSummary();
            parameterDescriptions = docComment.GetParams().ToDictionary(x => x.Name, x => x.Description);
        }

        var parameters = new ToolParameter[methodSymbol.Parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var x = methodSymbol.Parameters[i];

            var typeName = x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            string? jsonSchemaType = null;
            string? jsonSchemaFormat = null;
            bool isCancellationToken = false;
            if (typeName == "global::System.Threading.CancellationToken")
            {
                isCancellationToken = true;
            }
            else if (!TryGetJsonSchemaType(x.Type, out jsonSchemaType, out jsonSchemaFormat))
            {
                reporter.ReportDiagnostic(DiagnosticDescriptors.ToolParameterTypeIsNotSupported,
                    x.Type!.DeclaringSyntaxReferences[0].GetSyntax().GetLocation(),
                    typeName);
                return null;
            }

            parameters[i] = new ToolParameter
            {
                Type = typeName,
                Name = x.Name,
                JsonSchemaType = jsonSchemaType!,
                JsonSchemaFormat = jsonSchemaFormat,
                JsonSchemaDescription = parameterDescriptions?[x.Name],
                IsCancellationToken = isCancellationToken,
            };
        }

        return new ToolMetadata
        {
            Name = toolName,
            Description = summary ?? toolDescription,
            ReturnType = new(methodSymbol.ReturnType),
            ReturnTypeName = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            InvocationSymbol = methodSymbol.Name,
            Parameters = new(parameters),
            IsAsync = methodSymbol.IsAsync,
            NameSyntaxLocation = new(nameSyntaxLocation),
        };
    }

    public static ToolMetadata[] ParseFromClass(INamedTypeSymbol type, TypeDeclarationSyntax declarationSyntax, Location nameSyntaxLocation, DiagnosticReporter reporter)
    {
        if (type.DeclaringSyntaxReferences.Length == 0)
        {
            reporter.ReportDiagnostic(DiagnosticDescriptors.DefinedInOtherProject, declarationSyntax.GetLocation());
            return [];
        }

        var list = new List<ToolMetadata>();

        foreach (var method in type.GetMembers()
            .OfType<IMethodSymbol>())
        {
            var toolAttribute = method.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::McpToolkit.McpToolAttribute");
            if (toolAttribute == null) continue;

            var toolName = method.Name;
            if (toolAttribute != null)
            {
                toolName = toolAttribute.ConstructorArguments[0].Value!.ToString();
            }

            var metadata = ParseFromMethodCore(toolName, null, method, nameSyntaxLocation, reporter);
            if (metadata != null)
            {
                list.Add(metadata);
            }
        }

        return list.ToArray();
    }
}