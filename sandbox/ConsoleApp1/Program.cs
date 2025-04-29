using McpToolkit.Client;

await using var client = new McpClient();

await client.ConnectAsync(new StdioClientTransport()
{
    Command = "dotnet",
    Arguments = "run --project ../Server1/Server1.csproj",
});

await client.Ping.SendAsync();

await foreach (var tool in client.Tools.ListAsync())
{
    Console.WriteLine(tool);
}

var results = await client.Tools.CallAsync("add", new
{
    lhs = 1,
    rhs = 2,
});

foreach (var result in results)
{
    Console.WriteLine(result.Text);
}