using System.Text.Json;
using McpToolkit.JsonRpc;

namespace McpToolkit.Server;

public static class McpServerExtensions
{
    public static void Add(this IMcpServerTools tools, Tool tool, Func<JsonElement?, CancellationToken, ValueTask<Content[]>> handler)
    {
        tools.Add(new ToolDescriptor
        {
            Tool = tool,
            Handler = handler,
        });
    }

    public static void Add<T>(this IMcpServerTools tools)
        where T : IMcpToolProvider, new()
    {
        Add(tools, new T());
    }

    public static void Add<T>(this IMcpServerTools tools, T provider)
        where T : IMcpToolProvider
    {
        foreach (var descriptor in provider.GetToolDescriptors(tools.Server.ServiceProvider!))
        {
            tools.Add(descriptor);
        }
    }

    public static void Add(this IMcpServerResources resources, Resource resource, Func<string, CancellationToken, ValueTask<ResourceContents[]>> handler)
    {
        resources.Add(new ResourceDescriptor
        {
            Resource = resource,
            Handler = handler,
        });
    }

    public static void Add(this IMcpServerResources resources, string name, string uri, Func<string, CancellationToken, ValueTask<ResourceContents[]>> handler)
    {
        resources.Add(new()
        {
            Name = name,
            Uri = uri,
        }, handler);
    }

    public static void Add(this IMcpServerPrompts prompts, Prompt prompt, Func<JsonElement?, CancellationToken, ValueTask<PromptMessage[]>> handler)
    {
        prompts.Add(new PromptDescriptor
        {
            Prompt = prompt,
            Handler = handler,
        });
    }

    public static void SetRequestHandler<TParams, TResult>(this IMcpServer server, IRequestSchema<TParams, TResult> schema, Func<TParams?, CancellationToken, ValueTask<TResult>> handler)
    {
        server.SetRequestHandler(schema.Method, async (request, ct) =>
        {
            try
            {
                if (!server.IsConnected) throw new InvalidOperationException("Sserver is not connected");

                TParams? requestParams = default;
                if (request.Params != null)
                {
                    requestParams = JsonSerializer.Deserialize(request.Params!.Value, McpJsonSerializerContext.Default.Options.GetTypeInfo<TParams>());
                }
                var result = await handler(requestParams, ct);
                return new JsonRpcResponse
                {
                    Id = request.Id,
                    Result = JsonSerializer.SerializeToElement(result, McpJsonSerializerContext.Default.Options.GetTypeInfo<TResult>()),
                };
            }
            catch (McpException ex)
            {
                return new JsonRpcResponse
                {
                    Id = request.Id,
                    Error = new()
                    {
                        Code = (int)(ex.ErrorCode ?? JsonRpcErrorCode.InternalError),
                        Message = ex.Message
                    }
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

    public static void SetNotificationHandler<TParams>(this IMcpServer server, INotificationSchema<TParams> schema, Func<TParams?, CancellationToken, ValueTask> handler)
    {
        server.SetNotificationHandler(schema.Method, async (request, ct) =>
        {
            if (!server.IsConnected) throw new InvalidOperationException("Server is not connected");

            TParams? requestParams = default;
            if (request.Params != null)
            {
                requestParams = JsonSerializer.Deserialize(request.Params!.Value, McpJsonSerializerContext.Default.Options.GetTypeInfo<TParams>());
            }

            await handler(requestParams, ct);
        });
    }
}