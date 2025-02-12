using Discord;
using Discord.WebSocket;

namespace Ollim.Bot.Services
{
    public interface ISendMessageService
    {
        Task ScheduleMessage(CancellationToken stoppingToken);
        Task SendDailyMessageAsync(ITextChannel textChannel, SocketSlashCommand command = null);
    }
}
