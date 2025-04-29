namespace McpToolkit;

public interface INotificationSchema<TNotification>
{
    string Method { get; }
}

public static class NotificationSchema
{
    sealed class Schema<TNotification>(string method) : INotificationSchema<TNotification>
    {
        public string Method => method;
    }

    public static readonly INotificationSchema<InitializedNotificationParams> InitializedNotificationSchema = new Schema<InitializedNotificationParams>(McpMethods.Notifications.Initialized);
    public static readonly INotificationSchema<CancelledNotificationParams> CancelledNotificationSchema = new Schema<CancelledNotificationParams>(McpMethods.Notifications.Cancelled);
    public static readonly INotificationSchema<PromptListChangedNotificationParams> PromptListChangedNotificationSchema = new Schema<PromptListChangedNotificationParams>(McpMethods.Notifications.PromptListChanged);
    public static readonly INotificationSchema<ToolListChangedNotificationParams> ToolListChangedNotificationSchema = new Schema<ToolListChangedNotificationParams>(McpMethods.Notifications.ToolListChanged);
    public static readonly INotificationSchema<ResourceListChangedNotificationParams> ResourceListChangedNotificationSchema = new Schema<ResourceListChangedNotificationParams>(McpMethods.Notifications.ResourceListChanged);
    public static readonly INotificationSchema<ResourceUpdatedNotificationParams> ResourceUpdatedNotificationSchema = new Schema<ResourceUpdatedNotificationParams>(McpMethods.Notifications.ResourceUpdated);
}