using System.Diagnostics;
using System.Threading.Channels;
using McpToolkit.JsonRpc;

namespace McpToolkit.Client;

public sealed class StdioClientTransport : IMcpTransport
{
    public required string Command { get; init; }
    public string? Arguments { get; init; }
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(5);
    public string? WorkingDirectory { get; init; }
    public IDictionary<string, string>? EnvironmentVariables { get; init; }

    readonly JsonRpcEndpoint endpoint;

    Process? process;
    Channel<string>? outputChannel;
    bool isConnected;

    public StdioClientTransport()
    {
        endpoint = new(
            async (ct) =>
            {
                var result = await outputChannel!.Reader.ReadAsync(ct);
                return result;
            },
            async (json, ct) =>
            {
                await process!.StandardInput.WriteLineAsync(json.AsMemory(), ct);
                await process.StandardInput.FlushAsync(ct);
            },
            (ex, ct) =>
            {
                // Ignore error
                return default;
            });
    }

    public bool IsConnected => isConnected;

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Transport is already connected");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = Command,
            Arguments = Arguments,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            ErrorDialog = false,
            WorkingDirectory = WorkingDirectory ?? Environment.CurrentDirectory,
        };

        if (EnvironmentVariables != null)
        {
            foreach (var kv in EnvironmentVariables)
            {
                startInfo.Environment[kv.Key] = kv.Value;
            }
        }

        process = new()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };

        outputChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true
        });

        var waitOutputDataCompleted = new TaskCompletionSource<object?>();

        void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                outputChannel?.Writer.TryWrite(e.Data);
            }
            else
            {
                waitOutputDataCompleted?.TrySetResult(null);
            }
        }

        process.OutputDataReceived += OnOutputDataReceived;

        process.Exited += async (sender, e) =>
        {
            await waitOutputDataCompleted.Task.ConfigureAwait(false);
            outputChannel.Writer.TryComplete();
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start MCP server process");
        }

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        _ = endpoint.ReadMessagesAsync(cancellationToken);

        isConnected = true;
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
        try
        {
            process?.KillTree();
        }
        finally
        {
            process?.Dispose();
        }

        outputChannel?.Writer.TryComplete(null);
        return default;
    }
}