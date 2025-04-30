using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace McpToolkit.JsonRpc;

public sealed class JsonRpcEndpoint(Func<CancellationToken, ValueTask<string?>> readFunc, Func<string, CancellationToken, ValueTask> writeFunc)
{
    readonly ConcurrentDictionary<RequestId, TaskCompletionSource<JsonRpcResponse>> pendingRequests = new();
    readonly ConcurrentDictionary<string, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>>> requestHandlers = new();
    readonly ConcurrentDictionary<string, Func<JsonRpcNotification, CancellationToken, ValueTask>> notificationHandlers = new();
    int nextRequestId = 0;

    public void SetRequestHandler(string method, Func<JsonRpcRequest, CancellationToken, ValueTask<JsonRpcResponse>> handler)
    {
        requestHandlers.TryAdd(method, handler);
    }

    public void SetNotificationHandler(string method, Func<JsonRpcNotification, CancellationToken, ValueTask> handler)
    {
        notificationHandlers.TryAdd(method, handler);
    }

    public async Task ReadMessagesAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var line = await readFunc(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!line.StartsWith('{') || !line.EndsWith('}')) continue; // skip non-json input

                var message = JsonSerializer.Deserialize(line, McpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()!);

                switch (message)
                {
                    case JsonRpcRequest request:
                        if (requestHandlers.TryGetValue(request.Method, out var requestHandler))
                        {
                            var response = await requestHandler(request, cancellationToken);
                            await writeFunc(JsonSerializer.Serialize(response, McpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()), cancellationToken);
                        }
                        else
                        {
                            await writeFunc(JsonSerializer.Serialize(new JsonRpcResponse
                            {
                                Id = request.Id,
                                Error = new()
                                {
                                    Code = (int)JsonRpcErrorCode.MethodNotFound,
                                    Message = $"Method '{request.Method}' is not available",
                                }
                            }, McpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>()), cancellationToken);
                        }
                        break;
                    case JsonRpcResponse response:
                        {
                            if (pendingRequests.TryRemove(response.Id, out var tcs))
                            {
                                tcs.TrySetResult(response);
                            }
                        }
                        break;
                    case JsonRpcNotification notification:
                        if (notificationHandlers.TryGetValue(notification.Method, out var notificationHandler))
                        {
                            await notificationHandler(notification, cancellationToken);
                        }
                        break;
                    default:
                        throw new McpException($"Invalid response type: {message?.GetType().Name}");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }

    public async ValueTask SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
    {
        if (message is JsonRpcRequest request && !request.Id.IsValid)
        {
            request.Id = Interlocked.Increment(ref nextRequestId);
        }

        var json = JsonSerializer.Serialize(message, McpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcMessage>());
        await writeFunc(json, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<JsonRpcResponse> SendRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.Id.IsValid) request.Id = Interlocked.Increment(ref nextRequestId);

        var json = JsonSerializer.Serialize(request, McpJsonSerializerContext.Default.Options.GetTypeInfo<JsonRpcRequest>());

        var tcs = new TaskCompletionSource<JsonRpcResponse>();
        pendingRequests.TryAdd(request.Id, tcs);

        await writeFunc(json, cancellationToken).ConfigureAwait(false);

        return await tcs.Task;
    }
}