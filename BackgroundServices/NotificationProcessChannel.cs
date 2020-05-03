using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AzureDevOpsMonitor.NotificationResponseTypes;

namespace AzureDevOpsMonitor.BackgroundServices
{
    public class NotificationProcessChannel
    {
        private const int maxMessagesInChannel = 100;
        private readonly Channel<PullRequestUpdate> _channel;
    
        public NotificationProcessChannel()
        {
            var options = new BoundedChannelOptions(maxMessagesInChannel)
            {
                SingleReader = true,
                SingleWriter = false
            };
        
            _channel = Channel.CreateBounded<PullRequestUpdate>(options);
        }
        public async Task<bool> AddNotificationAsync(PullRequestUpdate notification, CancellationToken ct)
        {
            while (await _channel.Writer.WaitToWriteAsync(ct) && !ct.IsCancellationRequested)
            {
                if(_channel.Writer.TryWrite(notification))
                {
                    return true;
                }

            }
            return false;
        }

        public IAsyncEnumerable<PullRequestUpdate> ReadAllAsync(CancellationToken ct = default) => _channel.Reader.ReadAllAsync(ct);
    }
}