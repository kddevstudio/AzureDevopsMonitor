using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsMonitor.BackgroundServices
{
    public class NotificationProcessService: BackgroundService
    {
        VssConnection connection;
        private NotificationProcessChannel processingChannel;
        private ServiceHooksPublisherHttpClient subscriptionClient;

        private List<Subscription> subscriptions = new List<Subscription>();

        private KestrelConfig kestrelConfig;

        private AzureDevOpsConfig azureDevopsConfig;

        private JWTConfig JWTOptions;

        public NotificationProcessService(NotificationProcessChannel processingChannel, IOptions<AzureDevOpsConfig> azureOptions, IOptions<KestrelConfig> kestrelOptions, IOptions<JWTConfig> JWTOptions)
        {
            this.processingChannel = processingChannel;
            this.kestrelConfig = kestrelOptions.Value;
            this.azureDevopsConfig = azureOptions.Value;
            this.JWTOptions = JWTOptions.Value;

            var credentials = new VssBasicCredential("", azureDevopsConfig.PatToken);
            connection = new VssConnection(new Uri(azureDevopsConfig.CollectionUri), credentials);
            subscriptionClient = connection.GetClient<ServiceHooksPublisherHttpClient>();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // create a subscription for PullRequestUpdated
            var projectHttpClient = connection.GetClient<ProjectHttpClient>();
            var teamProject = await projectHttpClient.GetProject(azureDevopsConfig.ProjectName);

            var receiverEndpoint = this.kestrelConfig.Endpoints.ContainsKey("Https") ? this.kestrelConfig.Endpoints["Https"] : this.kestrelConfig.Endpoints["Http"];

            var serverSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTOptions.serverSecret));

            var subscriptionParameters = new Subscription()
            {
                ConsumerId = "webHooks",
                ConsumerActionId = "httpRequest",
                ConsumerInputs = new Dictionary<string, string>
                {
                       { "url", $"{receiverEndpoint.Url.Substring(0, receiverEndpoint.Url.LastIndexOf(":"))}/EventReceiver"},
                       { "httpHeaders", $"Authorization:Bearer {GenerateToken(serverSecret)}"}
                },
                EventType = "git.pullrequest.updated",
                PublisherId = "tfs",
                PublisherInputs = new Dictionary<string, string>
                {
                    {"projectId", teamProject.Id.ToString()}
                },
            };
        
            var subscription = await subscriptionClient.CreateSubscriptionAsync(subscriptionParameters);

            subscriptions.Add(subscription);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var gitHttpClient = connection.GetClient<GitHttpClient>();
            
            await foreach(var notification in processingChannel.ReadAllAsync())
            {
                switch(notification.EventType)
                {
                    case "git.pullrequest.updated":

                    var repositoryId = notification.Resource.Repository.Id;
                    var commitId = notification.Resource.Commits.FirstOrDefault().CommitId;

                    var changes = await gitHttpClient.GetChangesAsync(commitId, repositoryId);

                    break;          
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            subscriptions.ForEach(subscription => {
                subscriptionClient.DeleteSubscriptionAsync(subscription.Id);
            });

            return base.StopAsync(cancellationToken);
        }

        private string GenerateToken(SecurityKey key)
        {
            var now = DateTime.UtcNow;
            var issuer = JWTOptions.Issuer;
            var audience = JWTOptions.Audience;
            var identity = new ClaimsIdentity();
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateJwtSecurityToken(issuer, audience, identity, now, now.Add(TimeSpan.FromHours(1)), now, signingCredentials);
            var encodedJwt = handler.WriteToken(token);
            return encodedJwt;
        }
    }
}