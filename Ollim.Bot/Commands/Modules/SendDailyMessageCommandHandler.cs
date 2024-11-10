using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Ollim.Bot.Services;

namespace Ollim.Bot.Commands.Modules
{
    public class SendDailyMessageCommandHandler : InteractionModuleBase
    {
        private readonly ISendMessageService _sendService;

        public SendDailyMessageCommandHandler(ISendMessageService sendService)
        {
            _sendService = sendService;
        }

        [SlashCommand("days_to", "Days until 01, 2026!")]
        public async Task SendDailyMessage()
        {
            var textChannel = Context.Channel as ITextChannel;

            if (textChannel == null) return;

            await DeferAsync();
            await _sendService.SendDailyMessageAsync(textChannel, Context.Interaction as SocketSlashCommand);
        }
    }
}
