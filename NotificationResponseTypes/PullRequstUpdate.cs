using System;
using AzureDevOpsMonitor.NotificationResponseTypes.Common;

namespace AzureDevOpsMonitor.NotificationResponseTypes
{
    public class PullRequestUpdate: ServiceHookNotification
    {
        public Message Message {get ; set;}
        public DetailedMessage DetailedMessage {get; set;}
        public Resource Resource {get;set;}
        public ResourceContainers ResourceContainers {get;set;}

        public DateTime CreatedDate {get;set;}
    }
}