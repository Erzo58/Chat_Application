namespace ChatApp.Server.Api.Hub
{
    using ChatApp.Server.Core.Interfaces;
    using ChatApp.Server.Core.Model;
    using Microsoft.AspNetCore.SignalR;
    using System;
    using System.Threading.Tasks;

    public class ChatHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        public ChatHub(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task SendMessage(ChatMessage message)
        {

            message.Timestamp = DateTime.UtcNow;

            try
            {
                await _messageRepository.SaveMessageAsync(message);

                await Clients.All.SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
               await Clients.Caller.SendAsync("ErrorMessage", $"Chyba pri ukladaní: {ex.Message}");
            }
        }
    }
}