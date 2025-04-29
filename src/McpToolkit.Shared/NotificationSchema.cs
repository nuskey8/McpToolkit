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

    public static readonly INotificationSchema<InitializedNotificationParams> InitializedNotification = new Schema<InitializedNotificationParams>(McpMethods.Notifications.Initialized);
    public static readonly INotificationSchema<CancelledNotificationParams> CancelledNotification = new Schema<CancelledNotificationParams>(McpMethods.Notifications.Cancelled);
    public static readonly INotificationSchema<PromptListChangedNotificationParams> PromptListChangedNotification = new Schema<PromptListChangedNotificationParams>(McpMethods.Notifications.PromptListChanged);
    public static readonly INotificationSchema<ToolListChangedNotificationParams> ToolListChangedNotification = new Schema<ToolListChangedNotificationParams>(McpMethods.Notifications.ToolListChanged);
    public static readonly INotificationSchema<ResourceListChangedNotificationParams> ResourceListChangedNotification = new Schema<ResourceListChangedNotificationParams>(McpMethods.Notifications.ResourceListChanged);
    public static readonly INotificationSchema<ResourceUpdatedNotificationParams> ResourceUpdatedNotification = new Schema<ResourceUpdatedNotificationParams>(McpMethods.Notifications.ResourceUpdated);
}