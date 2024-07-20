using Discord;
using Discord.Interactions;
using Ollim.Bot.Services;
using Ollim.Infrastructure.Data;

namespace Ollim.Bot.Commands.Modules
{
    public class SetTextChannelCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AppDbContext _context;
        private ISendMessageService _sendMessageService;

        public SetTextChannelCommandHandler(AppDbContext context, ISendMessageService sendMessageService)
        {
            _context = context;
            _sendMessageService = sendMessageService;
        }

        [SlashCommand("set_channel", "Select the channel to send notifications to")]
        [RequireUserPermission(GuildPermission.Administrator)]

        public async Task SetTextChannelAsync(ITextChannel channel)
        {
            var guild = Context.Guild;
            var _channel = channel;
            _context.SetNotificationChannel(guild.Id, _channel.Id);


            _sendMessageService.ScheduleDailyMessage(_channel);

            await RespondAsync($"Canal de texto configurado para: {channel.Name}");
        }
    }
}
