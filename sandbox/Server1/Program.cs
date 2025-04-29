using McpToolkit.Server;
using McpToolkit;

await using var server = new McpServer();

server.Tools.Add<Tools>();

server.Tools.Add("add", "Add two numbers together.", (double lhs, double rhs) =>
{
    return lhs + rhs;
});

await server.ConnectAsync(new StdioServerTransport());

Console.Error.WriteLine("MCP Server (Server1) running on stdio");

await Task.Delay(Timeout.Infinite);

partial class Tools
{
    /// <summary>
    /// Rolls a dice and returns a random value between 1 and 6.
    /// </summary>
    [McpTool("roll_dice")]
    public static string RollDice()
    {
        return Random.Shared.Next(0, 7).ToString();
    }
}