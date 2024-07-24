using Discord;

namespace Ollim.Bot.Services
{
    public interface ISendMessageService
    {
        void ScheduleDailyMessage(ITextChannel textChannel);
    }
}
