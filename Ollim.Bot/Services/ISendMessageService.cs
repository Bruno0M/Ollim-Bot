using Discord;
using Discord.WebSocket;

namespace Ollim.Bot.Services
{
    public interface ISendMessageService
    {
        void SetTextChannel(ITextChannel textChannel);
        Task ScheduleMessage(CancellationToken stoppingToken);
        Task SendDailyMessageAsync(ITextChannel textChannel, SocketSlashCommand command = null);
    }
}
