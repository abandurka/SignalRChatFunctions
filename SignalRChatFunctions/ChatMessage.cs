using System;

namespace SignalRChatFunctions
{
    public class ChatMessage
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    }
}