using System.Threading.Channels;
using McpToolkit.JsonRpc;

namespace McpToolkit;

public sealed class InMemoryTransport : IMcpTransport
{
    readonly JsonRpcEndpoint endpoint;
    CancellationTokenSource? cts;
    bool isConnected;

    public bool IsConnected => isConnected;

    InMemoryTransport(JsonRpcEndpoint endpoint)
    {
        this.endpoint = endpoint;
    }

    public static (InMemoryTransport, InMemoryTransport) CreateLinkedPair()
    {
        var channel1 = Channel.CreateUnbounded<string>(new()
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true,
        });

        var channel2 = Channel.CreateUnbounded<string>(new()
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true,
        });

        var endpoint1 = new JsonRpcEndpoint(
            channel1.Reader.ReadAsync!,
            channel2.Writer.WriteAsync,
            (value, ct) => default);

        var endpoint2 = new JsonRpcEndpoint(
            channel2.Reader.ReadAsync!,
            channel1.Writer.WriteAsync,
            (value, ct) => default);

        return (new(endpoint1), new(endpoint2));
    }

    public ValueTask DisposeAsync()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }

        isConnected = false;
        return default;
    }

    public ValueTask SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
    {
        return endpoint.SendMessageAsync(message, cancellationToken);
    }

    public ValueTask<JsonRpcResponse> SendRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken = default)
    {
        return endpoint.SendRequestAsync(request, cancellationToken);
    }

    public void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler)
    {
        endpoint.SetNotificationHandler(method, handler);
    }

    public void SetRequestHandler(string method, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler)
    {
        endpoint.SetRequestHandler(method, handler);
    }

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        cts = new();

        Task.Run(async () =>
        {
            try
            {
                await endpoint.ReadMessagesAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }, default);

        isConnected = true;
        return default;
    }
}