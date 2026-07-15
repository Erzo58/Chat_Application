namespace ChatApp.Server.Core.Model
{
    public class ChatMessage
    {
        public string Username { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

    }
}
