using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace SignalRChatFunctions
{
    public class SignalRChatHubFunction
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        private readonly Lazy<CosmosClient> _cosmosClient = new Lazy<CosmosClient>(CreateClient);

        private static CosmosClient CreateClient()
        {
            var connectionString = Configuration["AzureCosmosDbConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("Please specify a valid endpoint in the appSettings.json file or your Azure Functions Settings.");
            }

            return new CosmosClientBuilder(connectionString).Build();
        }


        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "chat")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("messages")]
        public async Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] JObject message,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages
        )
        {
            var chatMessage = message.ToObject<ChatMessage>();
            await _cosmosClient.Value.GetContainer("SimpleChatDb", "MessageHistory").CreateItemAsync(chatMessage);
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "MessageReceived",
                    Arguments = new object[] { chatMessage }
                });
        }
    }
}
