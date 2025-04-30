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