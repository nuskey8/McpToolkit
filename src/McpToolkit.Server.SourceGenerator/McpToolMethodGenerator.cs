using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace McpToolkit.Server.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class McpToolMethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("GeneratedMcpServerToolsExtensions.g.cs", """
// <auto-generated />
#nullable enable
#pragma warning disable

namespace McpToolkit.Server
{
    internal static partial class GeneratedMcpServerToolsExtensions
    {
        public static partial void Add(this global::McpToolkit.Server.IMcpServerTools tools, string name, string? description, global::System.Delegate action);
    }
}
""");
        });

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, ct) =>
                {
                    if (node.IsKind(SyntaxKind.InvocationExpression))
                    {
                        if (node is not InvocationExpressionSyntax invocationExpression) return false;

                        var expr = invocationExpression.Expression as MemberAccessExpressionSyntax;
                        var methodName = expr?.Name.Identifier.Text;
                        if (methodName is "Add")
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }, static (context, ct) =>
                {
                    var reporter = new DiagnosticReporter();
                    var result = new ToolContext
                    {
                        DiagnosticReporter = reporter,
                        Model = context.SemanticModel,
                    };

                    var node = (InvocationExpressionSyntax)context.Node;

                    var model = context.SemanticModel.GetTypeInfo((node.Expression as MemberAccessExpressionSyntax)!.Expression, ct);
                    if (model.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) is not "global::McpToolkit.Server.IMcpServerTools") return result;

                    if (node.ArgumentList.Arguments.Count != 3)
                    {
                        return result;
                    }

                    var nameArgument = node.ArgumentList.Arguments[0];
                    var descriptionArgument = node.ArgumentList.Arguments[1];
                    var actionArgument = node.ArgumentList.Arguments[2];

                    // check name
                    if (!nameArgument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                    {
                        reporter.ReportDiagnostic(DiagnosticDescriptors.ToolNameMustBeStringLiteral, nameArgument.GetLocation());
                        return result;
                    }
                    if (context.SemanticModel.GetTypeInfo(nameArgument.Expression).Type?.SpecialType is not SpecialType.System_String)
                    {
                        return result;
                    }

                    // check description
                    if (!descriptionArgument.Expression.IsKind(SyntaxKind.NullKeyword))
                    {
                        if (descriptionArgument.Expression.Kind() is not SyntaxKind.StringLiteralExpression)
                        {
                            reporter.ReportDiagnostic(DiagnosticDescriptors.ToolDescriptionMustBeNullOrStringLiteral, descriptionArgument.GetLocation());
                            return result;
                        }

                        if (context.SemanticModel.GetTypeInfo(descriptionArgument.Expression).Type?.SpecialType is not SpecialType.System_String)
                        {
                            return result;
                        }
                    }

                    var toolName = (nameArgument.Expression as LiteralExpressionSyntax)!.Token.ValueText;
                    var toolDescription = (descriptionArgument.Expression as LiteralExpressionSyntax)!.Token.ValueText;

                    if (actionArgument.Expression is ParenthesizedLambdaExpressionSyntax lambda)
                    {
                        result.MetadataList = [Parser.ParseFromLambda(toolName, toolDescription, lambda, nameArgument.GetLocation(), context.SemanticModel, reporter)];
                    }
                    else
                    {
                        var methodSymbols = context.SemanticModel.GetMemberGroup(actionArgument.Expression);
                        if (methodSymbols.Length == 0 || methodSymbols[0] is not IMethodSymbol methodSymbol) return result;
                        result.MetadataList = [Parser.ParseFromMethod(toolName, toolDescription, methodSymbol, node, nameArgument.GetLocation(), reporter)];
                    }

                    return result;
                })
            .Collect();

        context.RegisterSourceOutput(provider, EmitToolMethods);
    }
}