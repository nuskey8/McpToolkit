using McpToolkit.JsonRpc;

namespace McpToolkit;

public interface IMcpTransport : IAsyncDisposable
{
    bool IsConnected { get; }
    void SetRequestHandler(string method, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler);
    void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler);
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default);
    ValueTask<JsonRpcResponse> SendRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken = default);
}