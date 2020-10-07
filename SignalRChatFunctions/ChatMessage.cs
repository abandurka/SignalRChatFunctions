using System;
using Newtonsoft.Json;

namespace SignalRChatFunctions
{
    public class ChatMessage
    {
        [JsonProperty("id")]
        public Guid id { get; set; } = Guid.NewGuid();
        
        [JsonProperty("userName")]
        public string UserName { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("postedDate")]
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    }
}