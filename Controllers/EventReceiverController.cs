using System;
using System.Threading;
using System.Threading.Tasks;
using AzureDevOpsMonitor.BackgroundServices;
using AzureDevOpsMonitor.NotificationResponseTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureDevOpsMonitor.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class EventReceiverController: Controller
    {
        private NotificationProcessChannel processingChannel;

        public EventReceiverController(NotificationProcessChannel processingChannel)
        {   
            this.processingChannel = processingChannel;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PullRequestUpdate notification, CancellationToken ct)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            try
            {
                    await processingChannel.AddNotificationAsync(notification, cts.Token);

                    return Ok();

            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}