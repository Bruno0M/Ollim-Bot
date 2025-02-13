using Ollim.Bot.Entities;

namespace Ollim.Domain.Repositories
{
    public interface IChannelRepository
    {
        void SetNotification(ulong guildId, ulong channelId);
        Task<Channel> GetNotification(ulong guildId);
        Task<List<Channel>> GetAllNotification();
    }
}
