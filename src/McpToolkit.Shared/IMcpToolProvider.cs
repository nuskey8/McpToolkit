namespace McpToolkit;

public interface IMcpToolProvider
{
    ToolDescriptor[] GetToolDescriptors(IServiceProvider serviceProvider);
}