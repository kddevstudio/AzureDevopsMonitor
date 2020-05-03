using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AzureDevOpsMonitor
{
    public class KestrelConfig
    {
        public Dictionary<string, KestrelEndpoint> Endpoints { get; set; }
    }

    public class KestrelEndpoint
    {
        public string Url { get; set; }
    }
}

