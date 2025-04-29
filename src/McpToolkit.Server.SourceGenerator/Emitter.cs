namespace McpToolkit.Server.SourceGenerator;

internal static class Emitter
{
    public static void EmitInputSchema(CodeBuilder builder, ToolParameter[] jsonParameters, bool emitAsVariableDeclaration)
    {
        if (emitAsVariableDeclaration)
        {
            builder.AppendLine("var inputSchema = global::System.Text.Json.JsonDocument.Parse(");
        }
        else
        {
            builder.AppendLine("global::System.Text.Json.JsonDocument.Parse(");
        }
        builder.AppendLine("\"\"\"");
        builder.AppendLine("{");
        using (builder.BeginIndent())
        {
            builder.AppendLine("\"type\": \"object\",");
            builder.AppendLine("\"properties\": {");
            using (builder.BeginIndent())
            {
                for (int i = 0; i < jsonParameters.Length; i++)
                {
                    var parameter = jsonParameters[i];
                    builder.AppendLine($"\"{parameter.Name}\": {{");

                    var kv = new List<(string Key, string Value)>();
                    kv.Add(("type", parameter.JsonSchemaType));
                    if (parameter.JsonSchemaDescription != null) kv.Add(("description", parameter.JsonSchemaDescription));
                    if (parameter.JsonSchemaFormat != null) kv.Add(("format", parameter.JsonSchemaFormat));

                    using (builder.BeginIndent())
                    {
                        for (int n = 0; n < kv.Count; n++)
                        {
                            var (Key, Value) = kv[n];
                            var isLast = n == kv.Count - 1;
                            builder.AppendLine($"\"{Key}\": \"{Value}\"{(isLast ? "" : ",")}");
                        }
                    }

                    if (i != jsonParameters.Length - 1)
                    {
                        builder.AppendLine("},");
                    }
                    else
                    {
                        builder.AppendLine("}");
                    }
                }
            }
            builder.AppendLine("},");
            builder.AppendLine($"\"required\": [{string.Join(", ", jsonParameters.Select(x => $"\"{x.Name}\""))}]");
        }
        builder.AppendLine("}");
        builder.AppendLine($"\"\"\").RootElement{(emitAsVariableDeclaration ? ";" : "")}");
    }

    public static void EmitHandlerBody(CodeBuilder builder, ToolParameter[] jsonParameters, ToolMetadata meta)
    {
        if (jsonParameters.Length > 0)
        {
            using (builder.BeginBlock("if (args == null)"))
            {
                builder.AppendLine($"throw new global::McpToolkit.McpException(\"Missing required argument '{jsonParameters[0].Name}'\");");
            }
        }

        foreach (var parameter in jsonParameters)
        {
            builder.AppendLine($"var {parameter.Name} = args.Value!.GetProperty(\"{parameter.Name}\").Deserialize<{parameter.Type}>();");
        }

        var invocationArgs = meta.Parameters.Array.Select(x =>
        {
            if (x.IsCancellationToken) return "ct";
            return x.Name;
        });

        var invocationExpr = $"{(meta.IsAsync ? "await " : "")}{meta.InvocationSymbol}({string.Join(", ", invocationArgs)})";

        if (meta.ReturnType == "global::McpToolkit.Content[]")
        {
            if (meta.IsAsync) builder.AppendLine($"return {invocationExpr};");
            else builder.AppendLine($"return new({invocationExpr});");
        }
        else if (meta.ReturnType == "global::McpToolkit.Content")
        {
            if (meta.IsAsync) builder.AppendLine($"return [{invocationExpr}];");
            else builder.AppendLine($"return new([{invocationExpr}]);");
        }
        else if (meta.ReturnType == null)
        {
            builder.AppendLine($"{invocationExpr};");

            if (meta.IsAsync) builder.AppendLine("return [];");
            else builder.AppendLine("return new([]);");
        }
        else
        {
            if (meta.IsAsync) builder.AppendLine($"return [({invocationExpr}).ToString()];");
            else builder.AppendLine($"return new([({invocationExpr}).ToString()]);");
        }
    }
}