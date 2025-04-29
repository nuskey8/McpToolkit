using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using McpToolkit.JsonRpc;

namespace McpToolkit.Client;

public static class McpClientExtensions
{
    public static void SetRequestHandler<TParams, TResult>(this IMcpClient client, IRequestSchema<TParams, TResult> schema, Func<TParams, CancellationToken, ValueTask<TResult>> handler)
    {
        client.SetRequestHandler(schema.Method, async (request, ct) =>
        {
            if (!client.IsConnected) throw new InvalidOperationException("Client is not connected");

            try
            {
                var requestParams = JsonSerializer.Deserialize(request.Params!.Value, McpJsonSerializerContext.Default.Options.GetTypeInfo<TParams>());
                var result = await handler(requestParams!, ct);
                return new JsonRpcResponse
                {
                    Id = request.Id,
                    Result = JsonSerializer.SerializeToElement(result, McpJsonSerializerContext.Default.Options.GetTypeInfo<TResult>()),
                };
            }
            catch (Exception ex)
            {
                return new JsonRpcResponse
                {
                    Id = request.Id,
                    Error = new()
                    {
                        Code = (int)JsonRpcErrorCode.InternalError,
                        Message = ex.Message
                    }
                };
            }
        });
    }


    public static void SetNotificationHandler<TParams>(this IMcpClient client, INotificationSchema<TParams> schema, Func<TParams?, CancellationToken, ValueTask> handler)
    {
        client.SetNotificationHandler(schema.Method, async (request, ct) =>
        {
            if (!client.IsConnected) throw new InvalidOperationException("Client is not connected");

            TParams? requestParams = default;
            if (request.Params != null)
            {
                requestParams = JsonSerializer.Deserialize(request.Params!.Value, McpJsonSerializerContext.Default.Options.GetTypeInfo<TParams>());
            }

            await handler(requestParams, ct);
        });
    }

    [RequiresDynamicCode("")]
    [RequiresUnreferencedCode("")]
    public static ValueTask<Content[]> CallAsync(this IMcpClientTools tools, string name, object? arguments = null, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        return CallAsync(tools, name, JsonSerializer.SerializeToElement(arguments, options), cancellationToken);
    }

    public static async ValueTask<Content[]> CallAsync(this IMcpClientTools tools, string name, JsonElement? arguments = null, CancellationToken cancellationToken = default)
    {
        var result = await tools.CallAsync(new()
        {
            Name = name,
            Arguments = arguments,
        }, cancellationToken).ConfigureAwait(false);

        if (result.IsError)
        {
            throw new McpException(result.Content[0].Text!);
        }

        return result.Content;
    }

    public static async IAsyncEnumerable<Tool> ListAsync(this IMcpClientTools tools, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? cursor = null;
        do
        {
            var result = await tools.ListAsync(new ListToolsRequestParams()
            {
                Cursor = cursor
            }, cancellationToken).ConfigureAwait(false);

            foreach (var tool in result.Tools)
            {
                yield return tool;
            }

            cursor = result.NextCursor;
        }
        while (cursor != null);
    }

    public static async IAsyncEnumerable<Resource> ListAsync(this IMcpClientResources resources, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? cursor = null;
        do
        {
            var result = await resources.ListAsync(new ListResourcesRequestParams()
            {
                Cursor = cursor
            }, cancellationToken).ConfigureAwait(false);

            foreach (var resource in result.Resources)
            {
                yield return resource;
            }

            cursor = result.NextCursor;
        }
        while (cursor != null);
    }

    public static async IAsyncEnumerable<ResourceTemplate> ListTemplatesAsync(this IMcpClientResources resources, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? cursor = null;
        do
        {
            var result = await resources.ListTemplatesAsync(new ListResourceTemplatesRequestParams()
            {
                Cursor = cursor
            }, cancellationToken).ConfigureAwait(false);

            foreach (var resourceTemplate in result.ResourceTemplates)
            {
                yield return resourceTemplate;
            }

            cursor = result.NextCursor;
        }
        while (cursor != null);
    }

    public static async IAsyncEnumerable<Prompt> ListAsync(this IMcpClientPrompts prompts, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? cursor = null;
        do
        {
            var result = await prompts.ListAsync(new ListPromptsRequestParams()
            {
                Cursor = cursor
            }, cancellationToken).ConfigureAwait(false);

            foreach (var prompt in result.Prompts)
            {
                yield return prompt;
            }

            cursor = result.NextCursor;
        }
        while (cursor != null);
    }
}