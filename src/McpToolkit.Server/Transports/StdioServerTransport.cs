using McpToolkit.JsonRpc;

namespace McpToolkit.Server;

public sealed class StdioServerTransport : IMcpTransport
{
    readonly JsonRpcEndpoint endpoint = new(
        static async (ct) =>
        {
            return await Console.In.ReadLineAsync(ct);
        },
        static async (json, ct) =>
        {
            await Console.Out.WriteLineAsync(json.AsMemory(), ct);
            await Console.Out.FlushAsync(ct);
        });

    CancellationTokenSource cts = new();
    bool isConnected;

    public bool IsConnected => isConnected;

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Transport is already connected");
        }

        Task.Run(async () =>
        {
            try
            {
                await endpoint.ReadMessagesAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }, default);

        isConnected = true;

        return default;
    }

    public ValueTask SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();
        return endpoint.SendMessageAsync(message, cancellationToken);
    }

    public ValueTask<JsonRpcResponse> SendRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();
        return endpoint.SendRequestAsync(request, cancellationToken);
    }

    public void SetRequestHandler(string method, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler)
    {
        endpoint.SetRequestHandler(method, handler);
    }

    public void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler)
    {
        endpoint.SetNotificationHandler(method, handler);
    }

    public ValueTask DisposeAsync()
    {
        cts.Cancel();
        cts.Dispose();
        isConnected = false;
        return default;
    }

    void ThrowIfNotConnected()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Transport is not connected");
        }
    }
}