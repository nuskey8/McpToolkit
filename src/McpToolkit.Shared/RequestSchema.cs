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

    public static readonly IRequestSchema<InitializeRequestParams, InitializeResult> InitializeRequestSchema = new Schema<InitializeRequestParams, InitializeResult>(McpMethods.Initialize);
    public static readonly IRequestSchema<RequestParams, EmptyResult> PintRequestSchema = new Schema<RequestParams, EmptyResult>(McpMethods.Ping);

    public static readonly IRequestSchema<GetPromptRequestParams, GetPromptResult> GetPromptRequestSchema = new Schema<GetPromptRequestParams, GetPromptResult>(McpMethods.Prompts.Get);
    public static readonly IRequestSchema<ListPromptsRequestParams, ListPromptsResult> ListPromptsRequestSchema = new Schema<ListPromptsRequestParams, ListPromptsResult>(McpMethods.Prompts.List);

    public static readonly IRequestSchema<CallToolRequestParams, CallToolResult> CallToolRequestSchema = new Schema<CallToolRequestParams, CallToolResult>(McpMethods.Tools.Call);
    public static readonly IRequestSchema<ListToolsRequestParams, ListToolsResult> ListToolsRequestSchema = new Schema<ListToolsRequestParams, ListToolsResult>(McpMethods.Tools.List);

    public static readonly IRequestSchema<ReadResourceRequestParams, ReadResourceResult> ReadResourceRequestSchema = new Schema<ReadResourceRequestParams, ReadResourceResult>(McpMethods.Resources.Read);
    public static readonly IRequestSchema<ListResourcesRequestParams, ListResourcesResult> ListResourcesRequestSchema = new Schema<ListResourcesRequestParams, ListResourcesResult>(McpMethods.Resources.List);
    public static readonly IRequestSchema<ListResourceTemplatesRequestParams, ListResourceTemplatesResult> ListResourceTemplatesRequestSchema = new Schema<ListResourceTemplatesRequestParams, ListResourceTemplatesResult>(McpMethods.ResourcesTemplates.List);

    public static readonly IRequestSchema<ListRootsRequestParams, ListRootsResult> ListRootsRequestSchema = new Schema<ListRootsRequestParams, ListRootsResult>(McpMethods.Roots.List);

    public static readonly IRequestSchema<CompleteRequestParams, CompleteResult> CompleteRequestSchema = new Schema<CompleteRequestParams, CompleteResult>(McpMethods.Completion.Complete);
    public static readonly IRequestSchema<SetLevelRequestParams, EmptyResult> SetLevelRequestSchema = new Schema<SetLevelRequestParams, EmptyResult>(McpMethods.Logging.SetLevel);
    public static readonly IRequestSchema<CreateMessageRequestParams, CreateMessageResult> CreateMessageRequestSchema = new Schema<CreateMessageRequestParams, CreateMessageResult>(McpMethods.Sampling.CreateMessage);
}