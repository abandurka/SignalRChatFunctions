using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SignalRChatFunctions
{
    public class MainPageFunction
    {
        const string ContentDirectoryName = "Content";

        [FunctionName("MainPageFunction")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var pathToFile = Path.Combine(context.FunctionAppDirectory, ContentDirectoryName, "Index.html");
            response.Content = new StringContent(await File.ReadAllTextAsync(pathToFile));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
