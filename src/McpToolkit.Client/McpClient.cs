
using System.Collections.Concurrent;
using System.Text.Json;
using McpToolkit.JsonRpc;

namespace McpToolkit.Client;

public interface IMcpClient : IAsyncDisposable
{
    IMcpClientTools Tools { get; }
    IMcpClientResources Resources { get; }
    IMcpClientPrompts Prompts { get; }
    IMcpClientRoots Roots { get; }
    IMcpClientPing Ping { get; }
    IMcpClientLogging Logging { get; }
    IMcpClientCompletion Completion { get; }

    bool IsConnected { get; }

    ValueTask ConnectAsync(IMcpTransport transport, CancellationToken cancellationToken = default);
    void SetRequestHandler(string methodName, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler);
    void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler);
}

public interface IMcpClientTools
{
    ValueTask<CallToolResult> CallAsync(CallToolRequestParams request, CancellationToken cancellationToken = default);
    ValueTask<ListToolsResult> ListAsync(ListToolsRequestParams request, CancellationToken cancellationToken = default);
}

public interface IMcpClientResources
{
    ValueTask<ReadResourceResult> ReadAsync(ReadResourceRequestParams request, CancellationToken cancellationToken = default);
    ValueTask<ListResourcesResult> ListAsync(ListResourcesRequestParams request, CancellationToken cancellationToken = default);
    ValueTask<ListResourceTemplatesResult> ListTemplatesAsync(ListResourceTemplatesRequestParams request, CancellationToken cancellationToken = default);
}

public interface IMcpClientPrompts
{
    ValueTask<GetPromptResult> GetAsync(GetPromptRequestParams request, CancellationToken cancellationToken = default);
    ValueTask<ListPromptsResult> ListAsync(ListPromptsRequestParams request, CancellationToken cancellationToken = default);
}

public interface IMcpClientRoots
{
    void Add(Root root);
}

public interface IMcpClientPing
{
    ValueTask<EmptyResult> SendAsync(CancellationToken cancellationToken = default);
}

public interface IMcpClientLogging
{
    ValueTask<EmptyResult> SetLevelAsync(SetLevelRequestParams request, CancellationToken cancellationToken = default);
    void SetLogger(Action<LoggingMessageNotificationParams> logAction);
}

public interface IMcpClientCompletion
{
    ValueTask<CompleteResult> ComplateAsync(CompleteRequestParams request, CancellationToken cancellationToken = default);
}

public class McpClient : IMcpClient
{
    sealed class ClientTools(McpClient client) : IMcpClientTools
    {
        public ValueTask<CallToolResult> CallAsync(CallToolRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<CallToolRequestParams, CallToolResult>(McpMethods.Tools.Call, request, cancellationToken);
        }

        public ValueTask<ListToolsResult> ListAsync(ListToolsRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<ListToolsRequestParams, ListToolsResult>(McpMethods.Tools.List, request, cancellationToken);
        }
    }

    sealed class ClientResources(McpClient client) : IMcpClientResources
    {
        public ValueTask<ListResourcesResult> ListAsync(ListResourcesRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<ListResourcesRequestParams, ListResourcesResult>(McpMethods.Resources.List, request, cancellationToken);
        }

        public ValueTask<ListResourceTemplatesResult> ListTemplatesAsync(ListResourceTemplatesRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<ListResourceTemplatesRequestParams, ListResourceTemplatesResult>(McpMethods.ResourcesTemplates.List, request, cancellationToken);
        }

        public ValueTask<ReadResourceResult> ReadAsync(ReadResourceRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<ReadResourceRequestParams, ReadResourceResult>(McpMethods.Resources.Read, request, cancellationToken);
        }
    }

    sealed class ClientPrompts(McpClient client) : IMcpClientPrompts
    {
        public ValueTask<GetPromptResult> GetAsync(GetPromptRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<GetPromptRequestParams, GetPromptResult>(McpMethods.Prompts.Get, request, cancellationToken);
        }

        public ValueTask<ListPromptsResult> ListAsync(ListPromptsRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<ListPromptsRequestParams, ListPromptsResult>(McpMethods.Prompts.List, request, cancellationToken);
        }
    }

    sealed class ClientRoots(McpClient client) : IMcpClientRoots
    {
        public void Add(Root root)
        {
            if (client.Capabilities.Roots == null)
            {
                throw new InvalidOperationException("Client does not support roots");
            }

            client.roots.Add(root);

            if (client.Capabilities.Roots.ListChanged == true)
            {
                client.TrySendNotification(McpMethods.Notifications.RootListChanged, new RootsListChangedNotificationParams());
            }
        }
    }

    sealed class ClientPing(McpClient client) : IMcpClientPing
    {
        public ValueTask<EmptyResult> SendAsync(CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<RequestParams, EmptyResult>(McpMethods.Ping, new(), cancellationToken);
        }
    }

    sealed class ClientLogging(McpClient client) : IMcpClientLogging
    {
        public ValueTask<EmptyResult> SetLevelAsync(SetLevelRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<SetLevelRequestParams, EmptyResult>(McpMethods.Logging.SetLevel, request, cancellationToken);
        }

        public void SetLogger(Action<LoggingMessageNotificationParams> logAction)
        {
            client.logAction = logAction;
        }
    }

    sealed class ClientCompletion(McpClient client) : IMcpClientCompletion
    {
        public ValueTask<CompleteResult> ComplateAsync(CompleteRequestParams request, CancellationToken cancellationToken = default)
        {
            return client.SendRequestAsync<CompleteRequestParams, CompleteResult>(McpMethods.Completion.Complete, request, cancellationToken);
        }
    }

    public string Name { get; init; } = "";
    public string Version { get; init; } = "0.0.1";
    public string ProtocolVersion { get; init; } = "2025-03-26";
    public ClientCapabilities Capabilities { get; init; } = new()
    {
        Roots = new() { ListChanged = true },
        Sampling = new() { },
    };
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    readonly ConcurrentDictionary<string, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>>> requestHandlers = new();
    readonly ConcurrentDictionary<string, Func<JsonRpcNotification, CancellationToken, ValueTask>> notificationHandlers = new();

    readonly ConcurrentBag<Root> roots = new();

    IMcpTransport? transport;
    CancellationTokenSource clientCts = new();
    Action<LoggingMessageNotificationParams>? logAction;

    public IMcpClientTools Tools { get; }
    public IMcpClientResources Resources { get; }
    public IMcpClientPrompts Prompts { get; }
    public IMcpClientRoots Roots { get; }
    public IMcpClientPing Ping { get; }
    public IMcpClientLogging Logging { get; }
    public IMcpClientCompletion Completion { get; }

    public bool IsConnected => transport != null;

    public McpClient()
    {
        Tools = new ClientTools(this);
        Resources = new ClientResources(this);
        Prompts = new ClientPrompts(this);
        Roots = new ClientRoots(this);
        Ping = new ClientPing(this);
        Logging = new ClientLogging(this);
        Completion = new ClientCompletion(this);

        this.SetRequestHandler(RequestSchema.ListRootsRequest, DefaultListRootsHandler);
        this.SetNotificationHandler(NotificationSchema.LoggingMessageNotification, DefaultLoggingMessageHandler);

        logAction = x =>
        {
            Console.WriteLine($"{x.Level}: {(x.Logger == null ? "" : $"{x.Logger} - ")}{x.Data}");
        };
    }

    public void SetRequestHandler(string methodName, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler)
    {
        requestHandlers[methodName] = handler;
    }

    public void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler)
    {
        notificationHandlers[method] = handler;
    }

    public async ValueTask ConnectAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(clientCts.Token, cancellationToken);
        cts.CancelAfter(Timeout);

        try
        {
            foreach (var kv in requestHandlers)
            {
                transport.SetRequestHandler(kv.Key, kv.Value);
            }

            foreach (var kv in notificationHandlers)
            {
                transport.SetNotificationHandler(kv.Key, kv.Value);
            }

            await transport.StartAsync(cancellationToken);
            this.transport = transport;

            await transport.SendRequestAsync(new JsonRpcRequest
            {
                Method = McpMethods.Initialize,
                Params = JsonSerializer.SerializeToElement(new InitializeRequestParams
                {
                    ProtocolVersion = ProtocolVersion,
                    Capabilities = Capabilities,
                    ClientInfo = new()
                    {
                        Name = Name,
                        Version = Version,
                    }
                }, McpJsonSerializerContext.Default.Options.GetTypeInfo<InitializeRequestParams>())
            }, cts.Token).ConfigureAwait(false);

            await transport.SendMessageAsync(new JsonRpcNotification
            {
                Method = McpMethods.Notifications.Initialized,
            }, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(ex.Message, ex, cancellationToken);
            }
            else if (clientCts.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(McpClient));
            }
            else
            {
                throw new TimeoutException($"The request was canceled due to the configured Timeout of {Timeout.TotalSeconds} seconds elapsing.", ex);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        clientCts.Cancel();
        clientCts.Dispose();

        if (transport != null)
        {
            await transport.DisposeAsync();
            transport = null;
        }
    }

    ValueTask<ListRootsResult> DefaultListRootsHandler(ListRootsRequestParams requestParams, CancellationToken cancellationToken)
    {
        return new(new ListRootsResult
        {
            Roots = roots.ToArray(),
        });
    }

    ValueTask DefaultLoggingMessageHandler(LoggingMessageNotificationParams? notification, CancellationToken cancellationToken)
    {
        if (notification != null)
        {
            logAction?.Invoke(notification);
        }
        return default;
    }

    async ValueTask<TResult> SendRequestAsync<TParams, TResult>(string method, TParams requestParams, CancellationToken cancellationToken = default)
    {
        if (transport == null || !transport.IsConnected)
        {
            throw new InvalidOperationException("Client is not connected");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(clientCts.Token, cancellationToken);
        cts.CancelAfter(Timeout);

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
            else if (clientCts.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(McpClient));
            }
            else
            {
                throw new TimeoutException($"The request was canceled due to the configured Timeout of {Timeout.TotalSeconds} seconds elapsing.", ex);
            }
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
            }, clientCts.Token);
        }
    }
}