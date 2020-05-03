namespace AzureDevOpsMonitor.NotificationResponseTypes
{
    public class ServiceHookNotification
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public string PublisherId { get; set; }
        public string Scope { get; set; }
    }
}