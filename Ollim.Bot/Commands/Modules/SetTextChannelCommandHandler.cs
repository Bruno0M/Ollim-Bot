using Discord;
using Discord.Interactions;
using Ollim.Infrastructure.Data;

namespace Ollim.Bot.Commands.Modules
{
    public class SetTextChannelCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AppDbContext _context;

        public SetTextChannelCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        [SlashCommand("set_channel", "Select the channel to send notifications to")]
        [RequireUserPermission(GuildPermission.Administrator)]

        public async Task SetTextChannelAsync(ITextChannel channel)
        {
            var guildId = Context.Guild.Id;
            var channelId = channel.Id;
            _context.SetNotificationChannel(guildId, channelId);

            await RespondAsync($"Canal de texto configurado para: {channel.Name}");
        }
    }
}
