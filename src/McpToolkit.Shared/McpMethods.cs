namespace McpToolkit;

public static class McpMethods
{
    public const string Initialize = "initialize";
    public const string Ping = "ping";

    public static class Roots
    {
        public const string List = "roots/list";
    }

    public static class Tools
    {
        public const string List = "tools/list";
        public const string Call = "tools/call";
    }

    public static class Prompts
    {
        public const string List = "prompts/list";
        public const string Get = "prompts/get";
    }

    public static class Resources
    {
        public const string Read = "resources/read";
        public const string List = "resources/List";
        public const string Subscribe = "resources/subscribe";
        public const string Unsubscribe = "resources/unsubscribe";
    }

    public static class ResourcesTemplates
    {
        public const string List = "resources/templates/list";
    }

    public static class Notifications
    {
        public const string Initialized = "notifications/initialized";
        public const string ToolListChanged = "notifications/tools/list_changed";
        public const string PromptListChanged = "notifications/prompts/list_changed";
        public const string ResourceListChanged = "notifications/resources/list_changed";
        public const string ResourceUpdated = "notifications/resources/updated";
        public const string RootListChanged = "notifications/roots/list_changed";
        public const string Cancelled = "notifications/cancelled";
        public const string Progress = "notifications/progress";
        public const string Message = "notifications/message";
    }

    public static class Completion
    {
        public const string Complete = "completion/complete";
    }

    public static class Logging
    {
        public const string SetLevel = "logging/setlevel";
    }

    public static class Sampling
    {
        public const string CreateMessage = "sampling/createMessage";
    }
}