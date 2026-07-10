using ChatApp.Server.Core.Model;

namespace ChatApp.Server.Core.Interfaces
{
    public interface IMessageRepository
    {
        Task SaveMessageAsync(ChatMessage message);
    }
}
