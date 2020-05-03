namespace AzureDevOpsMonitor.NotificationResponseTypes.Common
{
    public class Repository
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string DefaultBranch { get; set; }
        public string RemoteUrl { get; set; }

    }
}