namespace McpToolkit;

public interface IRequestSchema<TParams, TResult>
{
    string Method { get; }
}

public static class RequestSchema
{
    sealed class Schema<TParams, TResult>(string method) : IRequestSchema<TParams, TResult>
    {
        public string Method => method;
    }

    public static readonly IRequestSchema<InitializeRequestParams, InitializeResult> InitializeRequest = new Schema<InitializeRequestParams, InitializeResult>(McpMethods.Initialize);
    public static readonly IRequestSchema<RequestParams, EmptyResult> PintRequest = new Schema<RequestParams, EmptyResult>(McpMethods.Ping);

    public static readonly IRequestSchema<GetPromptRequestParams, GetPromptResult> GetPromptRequest = new Schema<GetPromptRequestParams, GetPromptResult>(McpMethods.Prompts.Get);
    public static readonly IRequestSchema<ListPromptsRequestParams, ListPromptsResult> ListPromptsRequest = new Schema<ListPromptsRequestParams, ListPromptsResult>(McpMethods.Prompts.List);

    public static readonly IRequestSchema<CallToolRequestParams, CallToolResult> CallToolRequest = new Schema<CallToolRequestParams, CallToolResult>(McpMethods.Tools.Call);
    public static readonly IRequestSchema<ListToolsRequestParams, ListToolsResult> ListToolsRequest = new Schema<ListToolsRequestParams, ListToolsResult>(McpMethods.Tools.List);

    public static readonly IRequestSchema<ReadResourceRequestParams, ReadResourceResult> ReadResourceRequest = new Schema<ReadResourceRequestParams, ReadResourceResult>(McpMethods.Resources.Read);
    public static readonly IRequestSchema<ListResourcesRequestParams, ListResourcesResult> ListResourcesRequest = new Schema<ListResourcesRequestParams, ListResourcesResult>(McpMethods.Resources.List);
    public static readonly IRequestSchema<ListResourceTemplatesRequestParams, ListResourceTemplatesResult> ListResourceTemplatesRequest = new Schema<ListResourceTemplatesRequestParams, ListResourceTemplatesResult>(McpMethods.ResourcesTemplates.List);

    public static readonly IRequestSchema<ListRootsRequestParams, ListRootsResult> ListRootsRequest = new Schema<ListRootsRequestParams, ListRootsResult>(McpMethods.Roots.List);

    public static readonly IRequestSchema<CompleteRequestParams, CompleteResult> CompleteRequest = new Schema<CompleteRequestParams, CompleteResult>(McpMethods.Completion.Complete);
    public static readonly IRequestSchema<SetLevelRequestParams, EmptyResult> SetLevelRequest = new Schema<SetLevelRequestParams, EmptyResult>(McpMethods.Logging.SetLevel);
    public static readonly IRequestSchema<CreateMessageRequestParams, CreateMessageResult> CreateMessageRequest = new Schema<CreateMessageRequestParams, CreateMessageResult>(McpMethods.Sampling.CreateMessage);
}