// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace OnUserInOutFunction
{
    public static class UserInOutFunctions
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        private static readonly Lazy<CosmosClient> _cosmosClient = new Lazy<CosmosClient>(CreateClient);

        [FunctionName("UserConnected")]
        public static async Task OnUserConnected([EventGridTrigger] EventGridEvent eventGridEvent,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {
            if (!(eventGridEvent.Data is JObject data))
            {
                log.LogError($"Data is not JObject. Actual type is {eventGridEvent.Data.GetType()}");
                return;
            }
            var connectionInfo = data.ToObject<ClientConnectionInfo>();
            await _cosmosClient.Value.GetContainer("SimpleChatDb", "Users").CreateItemAsync(connectionInfo);

            var usersCount = _cosmosClient.Value.GetContainer("SimpleChatDb", "Users").GetItemLinqQueryable<ClientConnectionInfo>().Count();
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "ReceiveClientsCountUpdateMessage",
                    Arguments = new object[] { new OnlineUsersMessage
                    {
                        Count = usersCount
                    } }
                });
        }

        [FunctionName("UserDisconnected")]
        public static async Task OnUserDisconnected([EventGridTrigger] EventGridEvent eventGridEvent,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {
            if (!(eventGridEvent.Data is JObject data))
            {
                log.LogError($"Data is not JObject. Actual type is {eventGridEvent.Data.GetType()}");
                return;
            }
            var connectionInfo = data.ToObject<ClientConnectionInfo>();
            var user = _cosmosClient.Value.GetContainer("SimpleChatDb", "Users")
                .GetItemLinqQueryable<ClientConnectionInfo>()
                .FirstOrDefault(x => x.ConnectionId == connectionInfo.ConnectionId);
            if (user != null)
            {
                await _cosmosClient.Value.GetContainer("SimpleChatDb", "Users").DeleteItemAsync<ClientConnectionInfo>(user.Id, PartitionKey.None);
            }

            var usersCount = _cosmosClient.Value.GetContainer("SimpleChatDb", "Users").GetItemLinqQueryable<ClientConnectionInfo>().Count();
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "ReceiveClientsCountUpdateMessage",
                    Arguments = new object[] { new OnlineUsersMessage
                    {
                        Count = usersCount
                    } }
                });
        }


        private static CosmosClient CreateClient()
        {
            var connectionString = Configuration["AzureCosmosDbConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("Please specify a valid endpoint in the appSettings.json file or your Azure Functions Settings.");
            }

            return new CosmosClientBuilder(connectionString).Build();
        }
    }
}
