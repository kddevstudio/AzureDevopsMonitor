namespace AzureDevOpsMonitor.NotificationResponseTypes.Common
{
    public class Commit
    {
        public string CommitId { get; set; }
        public Author Author { get; set; }

        public Committer Committer { get; set; }

        public string Comment { get; set; }

        public string Url { get; set; }

    }
}