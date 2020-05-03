using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsMonitor
{
    public class JWTConfig
    {
        public string serverSecret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
