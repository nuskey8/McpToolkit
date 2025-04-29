using System.Collections.Concurrent;
using System.Text.Json;
using McpToolkit.JsonRpc;

namespace McpToolkit.Server;

public interface IMcpServer : IAsyncDisposable
{
    IServiceProvider? ServiceProvider { get; }

    IMcpServerPrompts Prompts { get; }
    IMcpServerTools Tools { get; }
    IMcpServerResources Resources { get; }

    bool IsConnected { get; }

    ValueTask ConnectAsync(IMcpTransport transport, CancellationToken cancellationToken = default);
    void SetRequestHandler(string methodName, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler);
    void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler);
}

public interface IMcpServerPrompts
{
    IMcpServer Server { get; }
    void Add(PromptDescriptor descriptor);
}

public interface IMcpServerTools
{
    IMcpServer Server { get; }
    void Add(ToolDescriptor descriptor);
}

public interface IMcpServerResources
{
    IMcpServer Server { get; }
    void Add(ResourceDescriptor descriptor);
    void AddTemplate(ResourceTemplateDescriptor descriptor);
}

public interface IMcpServerRoots
{
    ValueTask<ListRootsResult> ListAsync(ListRootsRequestParams request, CancellationToken cancellationToken = default);
}

public interface IMcpServerSampling
{
    ValueTask<CreateMessageResult> CreateMessageAsync(CreateMessageRequestParams request, CancellationToken cancellationToken = default);
}

public sealed class McpServer : IMcpServer
{
    sealed class ServerPrompts(McpServer server) : IMcpServerPrompts
    {
        public IMcpServer Server => server;

        public void Add(PromptDescriptor descriptor)
        {
            if (server.Capabilities.Prompts == null)
            {
                throw new InvalidOperationException("Server does not support prompts");
            }

            if (!server.prompts.TryAdd(descriptor.Prompt.Name, descriptor))
            {
                throw new InvalidOperationException($"Prompt '{descriptor.Prompt.Name}' is already registered");
            }

            if (server.Capabilities.Prompts.ListChanged == true)
            {
                server.TrySendNotification(McpMethods.Notifications.PromptListChanged, new PromptListChangedNotificationParams());
            }
        }
    }

    sealed class ServerTools(McpServer server) : IMcpServerTools
    {
        public IMcpServer Server => server;

        public void Add(ToolDescriptor descriptor)
        {
            if (server.Capabilities.Tools == null)
            {
                throw new InvalidOperationException("Server does not support tools");
            }

            if (!server.tools.TryAdd(descriptor.Tool.Name, descriptor))
            {
                throw new InvalidOperationException($"Tool '{descriptor.Tool.Name}' is already registered");
            }

            if (server.Capabilities.Tools.ListChanged == true)
            {
                server.TrySendNotification(McpMethods.Notifications.ToolListChanged, new ToolListChangedNotificationParams());
            }
        }
    }

    sealed class ServerResources(McpServer server) : IMcpServerResources
    {
        public IMcpServer Server => server;

        public void Add(ResourceDescriptor descriptor)
        {
            if (server.Capabilities.Resources == null)
            {
                throw new InvalidOperationException("Server does not support resources");
            }

            if (!server.resources.TryAdd(descriptor.Resource.Uri, descriptor))
            {
                throw new InvalidOperationException($"Resource '{descriptor.Resource.Uri}' is already registered");
            }

            if (server.Capabilities.Resources.ListChanged == true)
            {
                server.TrySendNotification(McpMethods.Notifications.ResourceListChanged, new ResourceListChangedNotificationParams());
            }
        }

        public void AddTemplate(ResourceTemplateDescriptor descriptor)
        {
            if (server.Capabilities.Resources == null)
            {
                throw new InvalidOperationException("Server does not support resources");
            }

            server.resourceTemplates.Add(descriptor);

            if (server.Capabilities.Resources.ListChanged == true)
            {
                server.TrySendNotification(McpMethods.Notifications.ResourceListChanged, new ResourceListChangedNotificationParams());
            }
        }
    }

    sealed class ServerRoots(McpServer server) : IMcpServerRoots
    {
        public ValueTask<ListRootsResult> ListAsync(ListRootsRequestParams request, CancellationToken cancellationToken = default)
        {
            return server.SendRequestAsync<ListRootsRequestParams, ListRootsResult>(McpMethods.Roots.List, request, cancellationToken);
        }
    }

    sealed class ServerSampling(McpServer server) : IMcpServerSampling
    {
        public ValueTask<CreateMessageResult> CreateMessageAsync(CreateMessageRequestParams request, CancellationToken cancellationToken = default)
        {
            return server.SendRequestAsync<CreateMessageRequestParams, CreateMessageResult>(McpMethods.Sampling.CreateMessage, request, cancellationToken);
        }
    }

    public string Name { get; init; } = "";
    public string Version { get; init; } = "0.0.1";
    public ServerCapabilities Capabilities { get; init; } = new()
    {
        Tools = new() { ListChanged = true },
        Resources = new() { ListChanged = true },
        Prompts = new() { ListChanged = true },
    };

    public IServiceProvider? ServiceProvider { get; init; }

    public IMcpServerPrompts Prompts { get; }
    public IMcpServerTools Tools { get; }
    public IMcpServerResources Resources { get; }
    public IMcpServerRoots Roots { get; }
    public IMcpServerSampling Sampling { get; }

    public bool IsConnected => transport != null;

    readonly ConcurrentDictionary<string, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>>> requestHandlers = new();
    readonly ConcurrentDictionary<string, Func<JsonRpcNotification, CancellationToken, ValueTask>> notificationHandlers = new();

    readonly ConcurrentDictionary<string, PromptDescriptor> prompts = new();
    readonly ConcurrentDictionary<string, ToolDescriptor> tools = new();
    readonly ConcurrentDictionary<string, ResourceDescriptor> resources = new();
    readonly ConcurrentBag<ResourceTemplateDescriptor> resourceTemplates = new();

    CancellationTokenSource serverCts = new();
    IMcpTransport? transport;

    public McpServer()
    {
        Prompts = new ServerPrompts(this);
        Tools = new ServerTools(this);
        Resources = new ServerResources(this);
        Roots = new ServerRoots(this);
        Sampling = new ServerSampling(this);

        this.SetRequestHandler(RequestSchema.InitializeRequest, DefaultInitializeHandler);
        this.SetRequestHandler(RequestSchema.PintRequest, DefaultPingHandler);
        this.SetRequestHandler(RequestSchema.ListPromptsRequest, DefaultListPromptsHandler);
        this.SetRequestHandler(RequestSchema.GetPromptRequest, DefaultGetPromptHandler);
        this.SetRequestHandler(RequestSchema.ListToolsRequest, DefaultListToolsHandler);
        this.SetRequestHandler(RequestSchema.CallToolRequest, DefaultCallToolHandler);
        this.SetRequestHandler(RequestSchema.ReadResourceRequest, DefaultReadResourceHandler);
        this.SetRequestHandler(RequestSchema.ListResourcesRequest, DefaultListResourcesHandler);
        this.SetRequestHandler(RequestSchema.ListResourceTemplatesRequest, DefaultListResourceTemplatesHandler);
    }

    public void SetRequestHandler(string method, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler)
    {
        requestHandlers[method] = handler;
    }

    public void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler)
    {
        notificationHandlers[method] = handler;
    }

    public async ValueTask ConnectAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
    {
        foreach (var kv in requestHandlers)
        {
            transport.SetRequestHandler(kv.Key, kv.Value);
        }

        foreach (var kv in notificationHandlers)
        {
            transport.SetNotificationHandler(kv.Key, kv.Value);
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token, cancellationToken);

        try
        {
            await transport.StartAsync(cts.Token);
            this.transport = transport;
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(ex.Message, ex, cancellationToken);
            }
            else
            {
                throw new OperationCanceledException(ex.Message, ex, serverCts.Token);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        serverCts.Cancel();
        serverCts.Dispose();

        if (transport != null)
        {
            await transport.DisposeAsync();
            transport = null;
        }

        if (ServiceProvider is IAsyncDisposable ad)
        {
            await ad.DisposeAsync();
        }
        else if (ServiceProvider is IDisposable d)
        {
            d.Dispose();
        }
    }

    void TrySendNotification<TParams>(string method, TParams? notificationParams)
    {
        if (IsConnected)
        {
            _ = transport!.SendMessageAsync(new JsonRpcNotification
            {
                Method = method,
                Params = notificationParams == null
                    ? null
                    : JsonSerializer.SerializeToElement(notificationParams, McpJsonSerializerContext.Default.Options.GetTypeInfo<TParams>()),
            }, serverCts.Token);
        }
    }

    static void ThrowIfParameterIsMisssing(RequestParams? requestParams)
    {
        if (requestParams == null) throw new McpException("Missing request params.");
    }

    ValueTask<InitializeResult> DefaultInitializeHandler(InitializeRequestParams? request, CancellationToken cancellationToken)
    {
        ThrowIfParameterIsMisssing(request);

        var result = new InitializeResult
        {
            ProtocolVersion = request!.ProtocolVersion,
            Capabilities = Capabilities,
            ServerInfo = new()
            {
                Name = Name,
                Version = Version,
            },
        };

        return new(result);
    }

    ValueTask<EmptyResult> DefaultPingHandler(RequestParams? request, CancellationToken cancellationToken)
    {
        return new(EmptyResult.Instance);
    }

    ValueTask<ListPromptsResult> DefaultListPromptsHandler(ListPromptsRequestParams? request, CancellationToken cancellationToken)
    {
        if (Capabilities.Prompts == null) throw new McpException("Server does not support prompts");

        var promptModels = prompts.Select(x => x.Value.Prompt).ToArray();
        return new(new ListPromptsResult
        {
            Prompts = promptModels,
        });
    }

    async ValueTask<GetPromptResult> DefaultGetPromptHandler(GetPromptRequestParams? request, CancellationToken cancellationToken)
    {
        if (Capabilities.Prompts == null) throw new McpException("Server does not support prompts");

        ThrowIfParameterIsMisssing(request);

        if (prompts.TryGetValue(request!.Name, out var descriptor))
        {
            foreach (var arg in descriptor.Prompt.Arguments ?? [])
            {
                if (!arg.Required.GetValueOrDefault()) continue;
                if (request.Arguments == null || request.Arguments.Value.TryGetProperty(arg.Name, out _))
                {
                    throw new McpException($"Missing required argument '{arg.Name}'", JsonRpcErrorCode.InvalidParams);
                }
            }

            return new()
            {
                Description = descriptor.Prompt.Description,
                Messages = await descriptor.Handler(request.Arguments, cancellationToken),
            };
        }

        throw new McpException("Prompt not found", JsonRpcErrorCode.InvalidParams);
    }

    ValueTask<ListToolsResult> DefaultListToolsHandler(ListToolsRequestParams? request, CancellationToken cancellationToken)
    {
        if (Capabilities.Tools == null) throw new McpException("Server does not support tools");

        ThrowIfParameterIsMisssing(request);

        var toolModels = tools.Select(x => x.Value.Tool).ToArray();

        return new(new ListToolsResult
        {
            Tools = toolModels,
        });
    }

    async ValueTask<CallToolResult> DefaultCallToolHandler(CallToolRequestParams? request, CancellationToken cancellationToken)
    {
        if (Capabilities.Tools == null) throw new McpException("Server does not support tools");

        ThrowIfParameterIsMisssing(request);

        if (tools.TryGetValue(request!.Name, out var tool))
        {
            var result = await tool.Handler(request.Arguments, cancellationToken);
            return new CallToolResult
            {
                Content = result,
            };
        }

        throw new McpException("Tool not found", JsonRpcErrorCode.InvalidParams);
    }

    async ValueTask<ReadResourceResult> DefaultReadResourceHandler(ReadResourceRequestParams? request, CancellationToken cancellationToken)
    {
        if (Capabilities.Resources == null) throw new McpException("Server does not support resources");

        ThrowIfParameterIsMisssing(request);

        if (resources.TryGetValue(request!.Uri, out var resource))
        {
            var result = await resource.Handler(request.Uri, cancellationToken);
            return new ReadResourceResult
            {
                Contents = result,
            };
        }

        foreach (var descriptor in resourceTemplates)
        {
            if (MatchUriTemplate(descriptor.ResourceTemplate.UriTemplate, request.Uri))
            {
                var result = await descriptor.Handler(request.Uri, cancellationToken);
                return new ReadResourceResult
                {
                    Contents = result,
                };
            }
        }

        throw new McpException("Resource not found", JsonRpcErrorCode.InvalidParams);
    }

    ValueTask<ListResourcesResult> DefaultListResourcesHandler(ListResourcesRequestParams? request, CancellationToken cancellationToken)
    {
        if (Capabilities.Resources == null) throw new McpException("Server does not support resources");

        var models = resources.Select(x => x.Value.Resource).ToArray();

        return new(new ListResourcesResult
        {
            Resources = models,
        });
    }

    ValueTask<ListResourceTemplatesResult> DefaultListResourceTemplatesHandler(ListResourceTemplatesRequestParams? request, CancellationToken cancellationToken)
    {
        if (Capabilities.Resources == null) throw new McpException("Server does not support resources");

        var templates = resourceTemplates.Select(x => x.ResourceTemplate).ToArray();

        return new(new ListResourceTemplatesResult
        {
            ResourceTemplates = templates,
        });
    }

    async ValueTask<TResult> SendRequestAsync<TParams, TResult>(string method, TParams requestParams, CancellationToken cancellationToken = default)
    {
        if (transport == null || !transport.IsConnected)
        {
            throw new InvalidOperationException("Server is not connected");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token, cancellationToken);

        try
        {
            var response = await transport.SendRequestAsync(new JsonRpcRequest
            {
                Method = method,
                Params = JsonSerializer.SerializeToElement(requestParams, McpJsonSerializerContext.Default.Options.GetTypeInfo<TParams>()),
            }, cts.Token).ConfigureAwait(false);

            if (response.Error != null)
            {
                throw new McpException(response.Error.Message);
            }

            return JsonSerializer.Deserialize(response.Result!.Value, McpJsonSerializerContext.Default.Options.GetTypeInfo<TResult>())!;
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(ex.Message, ex, cancellationToken);
            }
            else
            {
                throw new OperationCanceledException(ex.Message, ex, serverCts.Token);
            }
        }
    }

    // TODO: Implement all the features of UriTemplate
    static bool MatchUriTemplate(string template, string uri)
    {
        var templateSegments = template.Split('/');
        var uriSegments = uri.Split('/');

        if (templateSegments.Length != uriSegments.Length)
        {
            return false;
        }

        for (int i = 0; i < templateSegments.Length; i++)
        {
            if (templateSegments[i].StartsWith('{') && templateSegments[i].EndsWith('}'))
            {
                return true;
            }
            else if (!string.Equals(templateSegments[i], uriSegments[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return false;
    }
}