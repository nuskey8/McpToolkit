using McpToolkit.JsonRpc;

namespace McpToolkit;

public class McpException : Exception
{
    public McpException(string message, JsonRpcErrorCode? errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public McpException(string? message) : base(message)
    {
    }

    public JsonRpcErrorCode? ErrorCode { get; }
}

public class McpProcessException(int exitCode, string[] errorOutput) : McpException("Process returns error, ExitCode:" + exitCode + Environment.NewLine + string.Join(Environment.NewLine, errorOutput))
{
    public int ExitCode { get; } = exitCode;
    public string[] ErrorOutput { get; } = errorOutput;
}