using Discord;
using Discord.WebSocket;

namespace Ollim.Bot.Services
{
    public interface ISendMessageService
    {
        void ScheduleDailyMessage(ITextChannel textChannel);
    }
}
