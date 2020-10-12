using System;
using Newtonsoft.Json;

namespace OnUserInOutFunction
{
    public class ClientConnectionInfo
    {
        public string Id => ConnectionId;

        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; set; }
        [JsonProperty("hubName")]
        public string HubName { get; set; }
        [JsonProperty("connectionId")]
        public string ConnectionId { get; set; }
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}