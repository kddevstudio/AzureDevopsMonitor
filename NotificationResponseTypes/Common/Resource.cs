using System;

namespace AzureDevOpsMonitor.NotificationResponseTypes.Common
{
    public class Resource
    {
        public Commit[] Commits { get; set; }
        public RefUpdate[] RefUpdates { get; set; }
        public Repository Repository { get; set; }
        public PushedBy PushedBy { get; set; }
        public int PushId { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
    }
}